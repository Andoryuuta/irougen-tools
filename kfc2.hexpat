import std.mem;
import std.sys;
import std.core;

// Unfortunately this format is very array-instance heavy, which requires removing the
// default limits. This allows the pattern to run, but very slowly....
#pragma array_limit 0
#pragma pattern_limit 0

#pragma loop_limit 100000

// Matches internal keen::ResourceId definition.
struct KeenResourceId {
    u8 guid[16];
    u32 type_name_hash;
    u32 part_index;
    u32 reserved_0;
    u32 reserved_1;
} [[static]];

struct ContainerInfo {
    u32 container_size;
    u32 unk_field_4;
    u32 entry_count;
    u32 unk_field_c;
} [[static]];

struct ResourceBundle {
    u32 file_offset_start;
    u32 bundle_size;
    u32 entry_count;
} [[static]];

struct ResourcePriority {
    u32 priority;
} [[static]];

struct LookupTableEntry {
    u32 min_index;
    u32 max_index;
} [[static]];

struct ContentInfo {
    u32 content_size;

    /// Used to index into the ContainerEntryLookupTableEntries.
    /// e.g. `ContainerEntryLookupTableEntries[UnkLookupTableID & (ContainerEntryLookupTableEntries.Size-1)];
    u16 unk_lookup_table_id;

    u16 unk_field_6;
    
    /// Content Hash - this matches the keen::ContentHash.hash1 field (used by Resource types to reference content)
    u32 content_hash;
    u16 unk_field_c;
    u16 unk_content_info_table_index;
} [[static]];

struct ContentLocation {
    u64 location_data;
    u16 container_index = (location_data >> 48) [[export]];
    u64 file_offset = (location_data & 0x0000FFFFFFFFFFFF) [[export]];
    
    u32 block_offset;
    
    u32 content_data;
    u8 content_type = (content_data >> 24) [[export]];
    u32 block_count = (content_data & 0x00FFFFFF) [[export]];
};

struct ResourceInfo {
    KeenResourceId resource_id;
} [[static]];

struct ResourceLocation {
    u32 offset;
    u32 size;
} [[static]];

struct ReflectionTypeNameHash {
    /// This is the FNV1A-32 hash of the fully qualified type name, corresponding to the `hash1` field in the
    /// reflection data.
    /// (keen::HashKey32 internally)
    u32 type_name_hash;
} [[static]];

struct ReflectionTypeInfo {
    /// The source data / hash algorithm is unknown, but this corresponds to the `hash2` field in the
    /// reflection data.
    /// (keen::HashKey32 internally)
    u32 type_hash_2;
    u32 unk_field_4;
    u32 instance_count;
} [[static]];

struct KfcArchiveHeader {
    u32 magic;
    u32 data_start_offset;
    u32 field_8;
    u32 field_c;    
} [[static]];

struct KfcArchiveToc {
    // Section 0 - SVN Version tag
    u32 version_tag_offset_base = $ [[export]];
    u32 version_tag_offset;
    u32 version_tag_length;

    // Section 1 - Container Info Table
    u32 container_info_table_offset_base = $ [[export]];
    u32 container_info_table_offset;
    u32 container_info_table_count;

    // Section 2 - Unknown/Unused
    u32 unk_section_2_offset_base = $ [[export]];
    u32 unk_section_2_offset;
    u32 unk_section_2_count;
    std::assert(unk_section_2_offset == 0 && unk_section_2_count == 0, "unk_section_2 not supported!");
    
    // Section 3 - Unknown/Unused
    u32 unk_section_3_offset_base = $ [[export]];
    u32 unk_section_3_offset;
    u32 unk_section_3_count;
    std::assert(unk_section_3_offset == 0 && unk_section_3_count == 0, "unk_section_3 not supported!");
    
    // Section 4 - Resource Bundle Table
    u32 resource_bundle_entry_offset_base = $ [[export]];
    u32 resource_bundle_entry_offset;
    u32 resource_bundle_entry_count;
    std::assert(resource_bundle_entry_count == 1, "resource_bundle_entry_count != 1!");
    
    // Section 5 - Resource Priority Table
    u32 resource_priority_table_offset_base = $ [[export]];
    u32 resource_priority_table_offset;
    u32 resource_priority_table_count;
    
    // Section 6 - Content Lookup Table
    u32 content_lookup_table_offset_base = $ [[export]];
    u32 content_lookup_table_offset;
    u32 content_lookup_table_count;
    
    // Section 7 - Content Info Table
    u32 content_info_table_offset_base = $ [[export]];
    u32 content_info_table_offset;
    u32 content_info_table_count;
    
    // Section 8 - Content Location Table
    u32 content_location_table_offset_base = $ [[export]];
    u32 content_location_table_offset;
    u32 content_location_table_count;
    
    // Section 9 - Resource Lookup Table
    u32 resource_lookup_table_offset_base = $ [[export]];
    u32 resource_lookup_table_offset;
    u32 resource_lookup_table_count;
    
    // Section 10 - Resource Info Table
    u32 resource_info_table_offset_base = $ [[export]];
    u32 resource_info_table_offset;
    u32 resource_info_table_count;
    
    // Section 11 - Resource Location Table
    u32 resource_location_table_offset_base = $ [[export]];
    u32 resource_location_table_offset;
    u32 resource_location_table_count;
    
    // Section 12 - Reflection Type Lookup Table
    u32 reflection_type_lookup_table_offset_base = $ [[export]];
    u32 reflection_type_lookup_table_offset;
    u32 reflection_type_lookup_table_count;
    
    // Section 13 - Reflection Type Name Hash Table
    u32 reflection_type_name_hash_table_offset_base = $ [[export]];
    u32 reflection_type_name_hash_table_offset;
    u32 reflection_type_name_hash_table_count;
    
    // Section 14 - Reflection Type Info Table
    u32 reflection_type_info_table_offset_base = $ [[export]];
    u32 reflection_type_info_table_offset;
    u32 reflection_type_info_table_count;
};


// ImHex-ism as it doesn't allow `for` loops within structs,
// so we intead need to create a array of a struct type that
// is allowed to reference it's index and parent within the array.
struct ResourceDataEntry {
    u32 index = std::core::array_index();
    $ = parent.resource_bundle_entries[0].file_offset_start + parent.resource_location_table_entries[index].offset;
    //u8 resource_data[parent.resource_location_table_entries[index].size];
    std::mem::Bytes<parent.resource_location_table_entries[index].size> resource_data;
};


struct KfcArchive {
    KfcArchiveHeader header;
    KfcArchiveToc toc;

    // Section 0 - SVN Version tag
    $ = toc.version_tag_offset_base + toc.version_tag_offset;
    char version_tag[toc.version_tag_length];

    // Section 1 - Container Info Table
    $ = toc.container_info_table_offset_base + toc.container_info_table_offset;
    ContainerInfo container_info_entries[toc.container_info_table_count];

    // Section 2 - Unknown/Unused
    // Section 3 - Unknown/Unused

    // Section 4 - Resource Bundle Table
    $ = toc.resource_bundle_entry_offset_base + toc.resource_bundle_entry_offset;
    ResourceBundle resource_bundle_entries[toc.resource_bundle_entry_count];

    // Section 5 - Resource Priority Table
    $ = toc.resource_priority_table_offset_base + toc.resource_priority_table_offset;
    ResourcePriority resource_priority_entries[toc.resource_priority_table_count];

    // Section 6 - Content Lookup Table
    $ = toc.content_lookup_table_offset_base + toc.content_lookup_table_offset;
    LookupTableEntry content_lookup_table_entries[toc.content_lookup_table_count];

    // Section 7 - Content Info Table
    $ = toc.content_info_table_offset_base + toc.content_info_table_offset;
    ContentInfo content_info_table_entries[toc.content_info_table_count];

    // Section 8 - Content Location Table
    $ = toc.content_location_table_offset_base + toc.content_location_table_offset;
    ContentLocation content_location_table_entries[toc.content_location_table_count];

    // Section 9 - Resource Lookup Table
    $ = toc.resource_lookup_table_offset_base + toc.resource_lookup_table_offset;
    LookupTableEntry resource_lookup_table_entries[toc.resource_lookup_table_count];
    
    // Section 10 - Resource Info Table
    $ = toc.resource_info_table_offset_base + toc.resource_info_table_offset;
    ResourceInfo resource_info_table_entries[toc.resource_info_table_count];

    // Section 11 - Resource Location Table
    $ = toc.resource_location_table_offset_base + toc.resource_location_table_offset;
    ResourceLocation resource_location_table_entries[toc.resource_location_table_count];

    // Section 12 - Reflection Type Lookup Table
    $ = toc.reflection_type_lookup_table_offset_base + toc.reflection_type_lookup_table_offset;
    LookupTableEntry reflection_type_lookup_table_entries[toc.reflection_type_lookup_table_count];

    // Section 13 - Reflection Type Name Hash Table
    $ = toc.reflection_type_name_hash_table_offset_base + toc.reflection_type_name_hash_table_offset;
    ReflectionTypeNameHash reflection_type_name_hash_table_entries[toc.reflection_type_name_hash_table_count];

    // Section 14 - Reflection Type Info Table
    $ = toc.reflection_type_info_table_offset_base + toc.reflection_type_info_table_offset;
    ReflectionTypeInfo reflection_type_info_table_entries[toc.reflection_type_info_table_count];
    
    ResourceDataEntry resources[toc.resource_info_table_count];
  
};


KfcArchive file @ 0x0;
