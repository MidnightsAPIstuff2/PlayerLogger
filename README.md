# PlayerLogger

A script for gtag fangames. It logs every player in your rooms (ID + name) and posts the log to a Discord webhook. If someone joins with no name, it flags them so you can ban them by hand later.

## What it does

- When you join a room, it sends the room name and everyone in it (ID and name)
- When someone new joins, it logs them a couple seconds later (names don't always sync right away)
- If a player changes their name mid-room, it logs that too
- Anyone with an empty or whitespace-only name gets marked `BAN TODO`
- Reports go out through a queue so Discord doesn't rate limit you

Example of what shows up in Discord:

```
room: ABCD (3 players)
a1b2c3d4  |  gorillaguy
e5f6a7b8  |  <no name>
   ^ BAN TODO (empty name)
c9d0e1f2  |  mangoman
```

It does **not** ban anyone on its own. It only flags. The ban is up to you.

## Requirements

- Unity (made for Photon PUN2 based fangames)
- Photon PUN2 installed (`Photon.Pun` / `Photon.Realtime`)
- A Discord webhook URL

## Setup

1. Drop `PlayerLogger.cs` into your Unity project (anywhere under `Assets`)
2. Put it on a GameObject in your first scene, something that sticks around like your network manager
3. Make a webhook in Discord: Channel settings > Integrations > Webhooks > New Webhook > Copy URL
4. Paste the URL into the `Webhook Url` field in the inspector
5. Build and join a room, the first report should show up right away

## Settings

| Field | Default | What it does |
|---|---|---|
| Webhook Url | empty | Where reports get sent. Leave it empty and nothing is sent |
| Name Sync Wait | 2 | Seconds to wait after a player joins before checking their name |
| Send Gap | 1.1 | Seconds between webhook messages |

## Notes

- If your game doesn't use Photon auth, `UserId` can be empty. In that case the logger falls back to the actor number (like `actor7`), which only lasts for that room
- Backticks in names get swapped for `'` so nobody can break the Discord code block
- Messages over 1900 characters get trimmed
- **Keep your webhook URL private.** Anyone with it can post to your channel, so don't commit it to a public repo. Set it in the inspector on a scene or prefab that you don't push, or load it from somewhere else

## License

MIT, do whatever you want with it.
