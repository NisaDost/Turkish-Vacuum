## Turkish Vacuum Devices Corporate Website

[Visit TurkishVacuum.com](https://turkishvacuum.com/)

This corporate website represents 'Turkish Vacuum Devices and Technologies' and aims to showcase the company's services, technological capabilities, and industry expertise, while strengthening brand awareness and providing a professional digital identity.

<img width="730" height="440" alt="turkishvacuum" src="https://github.com/user-attachments/assets/7c55d791-f76f-4ff6-8965-7cc13eb2d8b5" />

### Languages

The site is served in Turkish at `/` and in English under `/en` (e.g. `/destek` ↔ `/en/support`). Page addresses for both languages are defined in `Models/SiteRoutes.cs`. Texts are written side by side in the views:

```cshtml
<h2>@T("Hizmetlerimiz", "Our Services")</h2>
```

### Instagram feed (most popular posts)

The "Sosyal Medyada Biz" section on the home page picks the most popular posts automatically, so the page does not need to be edited when new posts are shared on Instagram. A background service (`Services/InstagramSyncService.cs`) fetches the latest posts every 6 hours and shows the ones with the most likes and comments. Without a token, the four fixed posts in `Services/InstagramFeed.cs` are shown.

Setup (one time):

1. Switch the Instagram account to a **professional** account (Business or Creator) in the Instagram app.
2. At [developers.facebook.com](https://developers.facebook.com/apps) create an app, add the **Instagram** product, open *API setup with Instagram login*, add the Instagram account and generate an access token.
3. On the server, add the token to `appsettings.json` (this file is not in git) or set the `Instagram__AccessToken` environment variable in the systemd service, then restart the service:

   ```json
   "Instagram": {
     "AccessToken": "IGAA..."
   }
   ```

The token is valid for 60 days; the site refreshes it every week and stores the refreshed token in `App_Data/instagram-token.json`, so the service user needs write access to that folder. If the token ever expires (for example, the site was offline for more than 60 days), generate a new one and replace it in the configuration. The new token is picked up automatically.

Optional settings in the same section: `PostCount` (default 4), `CandidateCount` (how many recent posts are ranked, default 50), `RefreshMinutes` (default 360).
