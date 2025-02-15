# Irougen-Tools

Tools for interacting with Enshrouded files


## CLI

### Inspect KFC
```text
$ IrougenTools.CLI.exe info --input "C:\Program Files (x86)\Steam\steamapps\common\Enshrouded\enshrouded.kfc"
SVN Version: 637515
SVN Branch: ^/game38/branches/ea_update_05
SVN Timestamp: 1/29/2025 4:45:54 PM +00:00
Container Count: 32
Resource Count: 65354
Content Count: 106304
```

### Unpack
```text
$ IrougenTools.CLI.exe unpack --input "C:\Program Files (x86)\Steam\steamapps\common\Enshrouded\enshrouded.kfc" --output ./output
```

(This currently only includes the resources, not the contents)