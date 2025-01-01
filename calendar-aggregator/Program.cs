using CalendarAggregator.Source;
using CalendarAggregator.Util;
using Ical.Net;
using Ical.Net.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
// ReSharper disable StringLiteralTypo

var builder = WebApplication.CreateSlimBuilder(args);
var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();

List<Source> calendars = [];

foreach (var calendar in builder.Configuration.GetSection("Calendars").GetChildren())
{
    switch (calendar["Type"])
    {
        case "Ms365Group":
            var ms365GroupConfig = calendar.Get<Ms365GroupConfig>() ?? throw new InvalidConfigurationException("Invalid Ms365Group");
            calendars.Add(new Microsoft365GroupSource(ms365GroupConfig, GraphCredentials.Get(builder.Configuration, ms365GroupConfig.Ms365Cred)));
            break;
        case "Ms365Mailbox":
            var ms365MailboxConfig = calendar.Get<Ms365MailboxConfig>() ?? throw new InvalidConfigurationException("Invalid Ms365Mailbox");
            calendars.Add(new Microsoft365MailboxSource(ms365MailboxConfig, GraphCredentials.Get(builder.Configuration, ms365MailboxConfig.Ms365Cred)));
            break;
        case "Ics":
            var icsConfig = calendar.Get<IcsConfig>() ?? throw new InvalidConfigurationException("Invalid Ics");
            calendars.Add(new IcsSource(icsConfig));
            break;
        default:
            throw new InvalidConfigurationException($"Invalid calendar Type (found {calendar["Type"]})");
    }

    app.Logger.LogInformation($"Calendar \"{calendar["FriendlyName"]}\" configured as {calendar["Guid"]}");
}

app.MapGet("/calendar.ics", async (HttpContext context) =>
{
    List<string> requestedCalendars = [];
    if (context.Request.Query.TryGetValue("id", out var id))
    {
        requestedCalendars.AddRange(id.ToString().Split(","));
    }
    if (requestedCalendars.Count == 0)
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("ID missing");
        return;
    }

    var masterCalendar = new Calendar
    {
        ProductId = "-//WollongongSDA//Calendar Aggregator//EN" // subject to https://github.com/ical-org/ical.net/issues/531
    };
    masterCalendar.AddProperty("X-WR-CALNAME", "My Church Calendar");
    masterCalendar.AddProperty("REFRESH-INTERVAL;VALUE=DURATION", "P4H");
    masterCalendar.AddProperty("X-PUBLISHED-TTL", "P4H");
    masterCalendar.AddProperty("COLOR", "#FF8800");
    masterCalendar.AddProperty("IMAGE;VALUE=URI;DISPLAY=BADGE;FMTTYPE=image/png", $"{context.Request.Scheme}://{context.Request.Host}/favicon.png");
    masterCalendar.AddProperty("URL", $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}");

    foreach (var item in requestedCalendars)
    {
        // ReSharper disable once SimplifyLinqExpressionUseAll
        if (!Guid.TryParse(item, out var guid) || !calendars.Any(x => x.Guid == guid))
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("ID invalid");
            return;
        }
        var mapping = calendars.First(x => x.Guid == guid);

        var events = await mapping.GetEvents();
        events.ForEach(e => e.AddProperty("X-WSDA-MAP", mapping.Guid.ToString()));
        masterCalendar.Events.AddRange(events);
    }

    var serializer = new CalendarSerializer();
    var serializedCalendar = serializer.SerializeToString(masterCalendar);

    context.Response.StatusCode = 200;
    context.Response.ContentType = "text/calendar";
    await context.Response.WriteAsync(serializedCalendar);
});

app.MapGet("/config", async (HttpContext context) =>
{
    context.Response.ContentType = "application/json";
    await context.Response.WriteAsJsonAsync(calendars.Where(c => c.IsPrivate == false).Select(c => new { Name = c.FriendlyName, Recommended = c.IsRecommended, Id = c.Guid }));
});

app.MapFallbackToFile("/index.html");

app.Run();
