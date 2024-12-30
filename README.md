# Calendar Aggregator

## appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "GraphCredentials": {
  },
  "Calendars": [
  ]
}
```

> Better customisation is planned later

Graph credentials is a dictionary with a name and your App Registration details, ie:
```json
"GraphCredentials": {
  "MyApp": {
    "clientId": "your-client-id",
    "clientSecret": "your-client-secret",
    "tenantId": "your-tenant-id"
  }
}
```

Calendars is a list of calendars you want to aggregate (note some `Type`s have specific configuration), ie:
```json
"Calendars": [
  {
    "Type": "Ms365Group",
    "FriendlyName": "CalendarX",
    "Guid": "generate-some-id-here",
    "GroupId": "group-id",
    "Ms365GroupCred": "MyApp"
  },
  {
    "Type": "Ics",
    "FriendlyName": "CalendarY",
    "Guid": "generate-some-id-here",
    "IcsUrl": "https://example.com/calendar.ics"
  }
]
```
