# Ices Cosmic Exploration (ICE)

Repo: `https://puni.sh/api/repository/ice`

> **XivHub fork.** Custom Dalamud repo: `https://plugins.xivhub.net/pluginmaster.json`
> See [Fork changes](#fork-changes-xivhub) for what this build adds over upstream.

Welcome to the moon plugin that makes that dreadful grind of the moon into something that can be automated and made so much simplier.  
The overall purpose of this is to help you re-roll and grind out certain Cosmic Exploration missinos. From anywhere on trying to get gold on every mission for that title, to grinding out relic experience so you get your crafter/gathering tools. Or even if you're grinding up to 500k points on each and every class.

<img width="621" height="394" alt="image" src="https://github.com/user-attachments/assets/722c3e44-74d2-49d3-964a-de21e6a2cf87" />

Features:  
✔️ Re-roll until you get the missions that you would like to grind  
✔️ Allow it to automatically chose which one is most optimal with "Relic XP Grind" and find the missions that fit your relic experience needs so you can finish those tools  
✔️ Select at what point you would like to turn in a mission (Bronze, Silver, Gold) to select when you would like to turnin  
✔️ Be able to prioritize what classes you would like to farm with priority farm mode, and cycle through classes/mission types for weather/timed/sequence missions

Requirements: 
### Crafting
- Artisan | Repo: `https://github.com/PunishXIV/Artisan`

### Fishing
- Autohook | Repo: `https://github.com/InitialDet/AutoHook`
- How to auto-accept collectables: `https://github.com/PunishXIV/AutoHook/blob/main/AcceptCollectable.md`

### Gathering
Just need navmesh
- Vnavmesh | Repo: `https://github.com/awgil/ffxiv_navmesh`

There is an in plugin window that goes over the specifics of each kind/will also show give you buttons to install plugins if you don't have them already. 

## Fork changes (XivHub)

This fork tracks upstream and adds:

### Features
- **Relic XP-per-second mission scoring.** Upstream's Relic Grind picks the mission with the highest raw weighted XP; this build scores by useful XP divided by mission time, so it favours the fastest path to the XP you still need. Each eligible mission is explored a few times to gather real timing data before the bot commits to exploiting the best one; until then a structural estimate (from craft/gather counts) stands in for measured time.

### Performance
- Relic class/XP info (`Cosmic_ClassInfo`) is read from game memory at most once per frame instead of many times across UI widgets.
- The Cosmic Agenda gold-mission goal count is computed in a single pass instead of enumerating the filtered set twice with a throwaway allocation each frame.

### Building on Linux
- ECommons and Pictomancy resolve as NuGet packages; only OtterGui is a submodule (auto-initialised by an MSBuild target). Run with `-p:EnableWindowsTargeting=true` and `DALAMUD_HOME` pointing at your Dalamud dev libraries.
