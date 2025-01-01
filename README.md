# Calendar Aggregator

## appsettings.json

> Better whitelabelled customisation is planned later for both the frontend and backend.

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "GraphCredentials": {},
  "Calendars": []
}
```

GraphCredentials is a dictionary with a name and your App Registration details. You can use multiple App Registrations for different tenancies or consent configurations as required.

> Graph credentials require Application scopes using client secrets, delegated authentication is not supported nor intended to be. At the moment, the `Calendars.Read`, `User.Read.All`, and `Group.Read.All` scopes are required to be configured for the app registration and must be consented to for the entire organisation. There is a possibility of granular permissions by consenting to specific accounts for personal calendars, however this is not actively supported since we mainly rely on Group calendars.

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
    "Ms365Cred": "MyApp"
  },
  {
    "Type": "Ms365Mailbox",
    "FriendlyName": "CalendarY",
    "Guid": "generate-some-id-here",
    "MailboxId": "mailbox-id",
    "CalendarName": "Calendar",
    "Ms365Cred": "MyApp"
  },
  {
    "Type": "Ics",
    "FriendlyName": "CalendarZ",
    "Guid": "generate-some-id-here",
    "IcsUrl": "https://example.com/calendar.ics"
  }
]
```

| Property     | Description                         | Required | Types                    |
| ------------ | ----------------------------------- | -------- | ------------------------ |
| Type         | Determines type of calendar source  | ✅       | All                      |
| FriendlyName | Display name                        | ✅       | All                      |
| Guid         | Persistent identifier               | ✅       | All                      |
| GroupId      | Entra Object Id                     | ✅       | Ms365Group               |
| MailboxId    | Entra Object Id                     | ✅       | Ms365Mailbox             |
| CalendarName | Calendar Display Name (in Exchange) |          | Ms365Mailbox             |
| Ms365Cred    | `GraphCredentials` key              | ✅       | Ms365Group, Ms365Mailbox |
| IcsUrl       | URL to ICS file                     | ✅       | Ics                      |

Note that for Ms365Mailbox, the CalendarName defaults to Calendar, however it is recommended to be explicit.

It is easiest to find the MS365 Guids in **_Entra or PowerShell_**. These are labelled as an "Object Id" (enabled on the default Group table view, needs the column to be manually added on Users).

## Internal Notes

- **UPNs will _probably_ work, however it is highly recommended to use an Object Id to avoid confusion with changing UPNs each year.**
- Do **not** change Guids each year where possible _except_ for Custom type calendars (which should be rotated annually).
