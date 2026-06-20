# IIS deployment

## Server prerequisites

1. Enable IIS with:
   - Web Server
   - Common HTTP Features
   - Static Content
   - Management Console
2. Install the .NET 8 Hosting Bundle.
3. Restart IIS after installing the Hosting Bundle:

   ```powershell
   iisreset
   ```

## Publish

From the repository root:

```powershell
dotnet publish .\ERPWebApp.Server\ERPWebApp.Server.csproj `
  -c Release `
  -o .\artifacts\iis
```

The publish target builds Angular and places its output under `wwwroot`.

## Create the IIS site

1. Copy the contents of `artifacts\iis` to the server, for example:
   `C:\inetpub\ERPWebApp`.
2. Create an application pool named `ERPWebApp`.
3. Set the application pool's **.NET CLR version** to **No Managed Code**.
4. Create a site whose physical path is `C:\inetpub\ERPWebApp`.
5. Bind the required hostname and HTTPS certificate.
6. Give `IIS AppPool\ERPWebApp` read and execute permission on the site folder.

## Database access

The application currently uses Windows integrated SQL authentication. Grant the
application-pool identity access to SQL Server and `WebAppDB`, or override the
connection string with an IIS environment variable:

```text
ConnectionStrings__Default
```

For a local SQL Server instance, grant access to:

```text
IIS AppPool\ERPWebApp
```

## Production settings

Set these through IIS Configuration Editor, environment variables, or a secured
`appsettings.Production.json` file:

```text
ConnectionStrings__Default
Jwt__Key
Jwt__Issuer
Jwt__Audience
Stripe__Secret_Key
Stripe__Publish_Key
MSG91__AuthKey
MSG91__TemplateId
GooglePlace__api_key
```

After changing settings or files, recycle the application pool.

## Verification

Open:

```text
https://your-host/test
```

Expected response:

```text
API Running
```

Then open the site root to verify Angular routing and static assets.
