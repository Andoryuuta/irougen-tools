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

### Unpack KFC (resources)
This currently only includes the resources, not the contents

```text
$ IrougenTools.CLI.exe unpack --input "C:\Program Files (x86)\Steam\steamapps\common\Enshrouded\enshrouded.kfc" --output ./output
```

### Deserialize Resource
Requires the same reflection data as the .kfc file the resource was extracted from.

Outputs in a VERY temporary automatic XML format (please don't rely on this structure for anything).

```text
$ IrougenTools.CLI.exe deserialize-resource --types .\game-data\reflection_info.637515.json --input .\output\e5bb7aec-c3b5-4864-bc7f-6f14a4577ab4.0.keen-ecs-TemplateResource
```
Stdout output: 
```xml
<?xml version="1.0" encoding="utf-16"?>
<ReflectionNode xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" Name="_root" Type="keen::ecs::TemplateResource">
  <Children>
    <ReflectionNode Name="name" Type="keen::string">
      <Children>
        <ReflectionNode Name="_typedef" Type="keen::BlobString">
          <Children />
          <PrimitiveValue xsi:type="xsd:string">Test_Buff_Equipment</PrimitiveValue>
        </ReflectionNode>
      </Children>
    </ReflectionNode>
    <ReflectionNode Name="predictEntity" Type="keen::bool">
      <Children />
      <PrimitiveValue xsi:type="xsd:boolean">false</PrimitiveValue>
    </ReflectionNode>
    <ReflectionNode Name="farCulling" Type="keen::bool">
      <Children />
      <PrimitiveValue xsi:type="xsd:boolean">false</PrimitiveValue>
    </ReflectionNode>
    <ReflectionNode Name="omitTransformReplication" Type="keen::bool">
      <Children />
      <PrimitiveValue xsi:type="xsd:boolean">false</PrimitiveValue>
    </ReflectionNode>
    <ReflectionNode Name="components" Type="keen::BlobArray&lt;keen::BlobVariant&lt;keen::ecs::Component&gt;&gt;">
      <Children>
        <ReflectionNode Name="_blobArray[0]" Type="keen::BlobVariant&lt;keen::ecs::Component&gt;">
          <Children>
            <ReflectionNode Name="_blobVariant" Type="keen::ecs::Impact">
              <Children>
                <ReflectionNode Name="_base" Type="keen::ecs::Component">
                  <Children />
                </ReflectionNode>
                <ReflectionNode Name="program" Type="keen::ImpactProgramReference">
                  <Children>
                    <ReflectionNode Name="_typedef" Type="keen::ObjectReference&lt;keen::ImpactDefinition&gt;">
                      <Children />
                      <PrimitiveValue xsi:type="xsd:base64Binary">qIGoSloEpE6hwDHdwYSq7g==</PrimitiveValue>
                    </ReflectionNode>
                  </Children>
                </ReflectionNode>
                <ReflectionNode Name="impactId" Type="keen::impact::ImpactId">
                  <Children>
                    <ReflectionNode Name="id" Type="keen::uint32">
                      <Children />
                      <PrimitiveValue xsi:type="xsd:unsignedInt">0</PrimitiveValue>
                    </ReflectionNode>
                  </Children>
                </ReflectionNode>
              </Children>
            </ReflectionNode>
          </Children>
        </ReflectionNode>
        <ReflectionNode Name="_blobArray[1]" Type="keen::BlobVariant&lt;keen::ecs::Component&gt;">
          <Children>
            <ReflectionNode Name="_blobVariant" Type="keen::ecs::ImpactConfiguration">
              <Children>
                <ReflectionNode Name="_base" Type="keen::ecs::Component">
                  <Children />
                </ReflectionNode>
                <ReflectionNode Name="configValues" Type="keen::impact::Configurations">
                  <Children>
                    <ReflectionNode Name="_typedef" Type="keen::BlobArray&lt;keen::BlobVariant&lt;keen::impact::ImpactConfig&gt;&gt;">
                      <Children />
                    </ReflectionNode>
                  </Children>
                </ReflectionNode>
                <ReflectionNode Name="damageDistribution" Type="keen::impact::DamageDistribution">
                  <Children>
                    <ReflectionNode Name="physicalCutDamage" Type="keen::float32">
                      <Children />
                      <PrimitiveValue xsi:type="xsd:float">0</PrimitiveValue>
                    </ReflectionNode>
                    <ReflectionNode Name="physicalPierceDamage" Type="keen::float32">
                      <Children />
                      <PrimitiveValue xsi:type="xsd:float">0</PrimitiveValue>
                    </ReflectionNode>
                    <ReflectionNode Name="physicalBluntDamage" Type="keen::float32">
                      <Children />
                      <PrimitiveValue xsi:type="xsd:float">0</PrimitiveValue>
                    </ReflectionNode>
                    <ReflectionNode Name="magicalFireDamage" Type="keen::float32">
                      <Children />
                      <PrimitiveValue xsi:type="xsd:float">0</PrimitiveValue>
                    </ReflectionNode>
                    <ReflectionNode Name="magicalIceDamage" Type="keen::float32">
                      <Children />
                      <PrimitiveValue xsi:type="xsd:float">0</PrimitiveValue>
                    </ReflectionNode>
                    <ReflectionNode Name="magicalFogDamage" Type="keen::float32">
                      <Children />
                      <PrimitiveValue xsi:type="xsd:float">0</PrimitiveValue>
                    </ReflectionNode>
                    <ReflectionNode Name="magicalLightningDamage" Type="keen::float32">
                      <Children />
                      <PrimitiveValue xsi:type="xsd:float">0</PrimitiveValue>
                    </ReflectionNode>
                    <ReflectionNode Name="healing" Type="keen::float32">
                      <Children />
                      <PrimitiveValue xsi:type="xsd:float">0</PrimitiveValue>
                    </ReflectionNode>
                    <ReflectionNode Name="poisonDamage" Type="keen::float32">
                      <Children />
                      <PrimitiveValue xsi:type="xsd:float">0</PrimitiveValue>
                    </ReflectionNode>
                    <ReflectionNode Name="woodDamage" Type="keen::uint32">
                      <Children />
                      <PrimitiveValue xsi:type="xsd:unsignedInt">0</PrimitiveValue>
                    </ReflectionNode>
                    <ReflectionNode Name="stoneDamage" Type="keen::uint32">
                      <Children />
                      <PrimitiveValue xsi:type="xsd:unsignedInt">0</PrimitiveValue>
                    </ReflectionNode>
                    <ReflectionNode Name="metalDamage" Type="keen::uint32">
                      <Children />
                      <PrimitiveValue xsi:type="xsd:unsignedInt">0</PrimitiveValue>
                    </ReflectionNode>
                    <ReflectionNode Name="explosionDamage" Type="keen::uint32">
                      <Children />
                      <PrimitiveValue xsi:type="xsd:unsignedInt">0</PrimitiveValue>
                    </ReflectionNode>
                  </Children>
                </ReflectionNode>
                <ReflectionNode Name="damageDistributionIsSet" Type="keen::bool">
                  <Children />
                  <PrimitiveValue xsi:type="xsd:boolean">false</PrimitiveValue>
                </ReflectionNode>
              </Children>
            </ReflectionNode>
          </Children>
        </ReflectionNode>
        <ReflectionNode Name="_blobArray[2]" Type="keen::BlobVariant&lt;keen::ecs::Component&gt;">
          <Children>
            <ReflectionNode Name="_blobVariant" Type="keen::ecs::StaticTransform">
              <Children>
                <ReflectionNode Name="_base" Type="keen::ecs::Component">
                  <Children />
                </ReflectionNode>
              </Children>
            </ReflectionNode>
          </Children>
        </ReflectionNode>
      </Children>
    </ReflectionNode>
  </Children>
</ReflectionNode>
```