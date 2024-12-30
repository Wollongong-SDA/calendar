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

var builder = WebApplication.CreateSlimBuilder(args);
var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();

List<Source> calendarMappings = [];

foreach (var calendar in builder.Configuration.GetSection("Calendars").GetChildren())
{
    switch (calendar["Type"])
    {
        case "Ms365Group":
            var ms365GroupConfig = calendar.Get<Ms365GroupConfig>() ?? throw new InvalidConfigurationException("Invalid Ms365Group");
            calendarMappings.Add(new Microsoft365GroupSource(GraphCredentials.Get(builder.Configuration, ms365GroupConfig.Ms365GroupCred))
            {
                FriendlyName = ms365GroupConfig.FriendlyName,
                Guid = new Guid(ms365GroupConfig.Guid),
                GroupId = ms365GroupConfig.GroupId
            });
            break;
        case "Ics":
            var icsConfig = calendar.Get<IcsConfig>() ?? throw new InvalidConfigurationException("Invalid Ics");
            calendarMappings.Add(new IcsSource()
            {
                FriendlyName = icsConfig.FriendlyName,
                Guid = new Guid(icsConfig.Guid),
                IcsUrl = icsConfig.IcsUrl
            });
            break;
        default:
            throw new InvalidConfigurationException($"Invalid calendar Type (found {calendar["Type"]})");
    }

    app.Logger.LogInformation($"Calendar \"{calendar["FriendlyName"]}\" configured as {calendar["Guid"]}");
}

app.MapGet("/calendar.ics", async (HttpContext context) =>
{
    List<string> calendars = [];
    if (context.Request.Query.TryGetValue("id", out var id))
    {
        calendars.AddRange(id.ToString().Split(","));
    }
    if (calendars.Count == 0)
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

    foreach (var item in calendars)
    {
        if (!Guid.TryParse(item, out var guid) || !calendarMappings.Any(x => x.Guid == guid))
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("ID invalid");
            return;
        }
        var mapping = calendarMappings.First(x => x.Guid == guid);

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

app.MapFallbackToFile("/index.html");

app.Run();
