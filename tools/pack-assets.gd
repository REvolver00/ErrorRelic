extends SceneTree

# Run with a Godot 4 runtime (the installed game executable also works):
# --headless --script pack-assets.gd -- <repository> <output.pck>
# Imports textures as portable .res resources and remaps the original PNG paths.
func _init():
    var args = OS.get_cmdline_user_args()
    if args.size() != 2:
        push_error("Expected repository directory and output .pck")
        quit(1)
        return
    var repository = args[0].replace("\\", "/")
    var output = args[1].replace("\\", "/")
    var temporary = output.get_base_dir().path_join("asset-import")
    DirAccess.make_dir_recursive_absolute(output.get_base_dir())
    DirAccess.make_dir_recursive_absolute(temporary)
    print("ERROR_ASSET_PACK_BEGIN: " + repository + " -> " + output)
    var packer = PCKPacker.new()
    if packer.pck_start(output) != OK:
        quit(1)
        return
    var files: Array[String] = []
    collect(repository.path_join("ErrorRelics"), files)
    # RestSiteOption.Icon uses this fixed native path, outside the mod namespace.
    collect(repository.path_join("images/ui/rest_site"), files)
    var custom_audio_names: Array[String] = []
    for candidate in files:
        var candidate_relative = candidate.trim_prefix(repository + "/")
        if candidate_relative.begins_with("ErrorRelics/audio/custom_sfx/") and (candidate.ends_with(".ogg") or candidate.ends_with(".wav")):
            custom_audio_names.append(candidate.get_file())
    for file in files:
        var relative = file.trim_prefix(repository + "/")
        if file.ends_with(".png"):
            var texture = ImageTexture.create_from_image(Image.load_from_file(file))
            var imported = temporary.path_join(relative.replace("/", "_") + ".res")
            var packed = "res://" + relative + ".res"
            if ResourceSaver.save(texture, imported) != OK:
                quit(1)
                return
            var remap = imported + ".remap"
            var writer = FileAccess.open(remap, FileAccess.WRITE)
            writer.store_string('[remap]\npath="' + packed + '"\n')
            writer.close()
            if packer.add_file(packed, imported) != OK or packer.add_file("res://" + relative + ".remap", remap) != OK:
                quit(1)
                return
        elif file.ends_with(".ogg") or file.ends_with(".wav"):
            var stream: AudioStream = null
            if file.ends_with(".ogg"):
                stream = AudioStreamOggVorbis.load_from_file(file)
            else:
                stream = AudioStreamWAV.load_from_file(file)
            if stream == null:
                push_error("Could not import audio: " + file)
                quit(1)
                return
            var imported_audio = temporary.path_join(relative.replace("/", "_") + ".res")
            var packed_audio = "res://" + relative + ".res"
            if ResourceSaver.save(stream, imported_audio) != OK:
                quit(1)
                return
            var audio_remap = imported_audio + ".remap"
            var audio_writer = FileAccess.open(audio_remap, FileAccess.WRITE)
            audio_writer.store_string('[remap]\npath="' + packed_audio + '"\n')
            audio_writer.close()
            if packer.add_file(packed_audio, imported_audio) != OK or packer.add_file("res://" + relative + ".remap", audio_remap) != OK:
                quit(1)
                return
        elif file.ends_with(".json") or file.ends_with(".txt"):
            if packer.add_file("res://" + relative, file) != OK:
                quit(1)
                return
    # Always add a manifest, even when custom_sfx is empty. Runtime discovery
    # therefore never depends on an empty directory surviving PCK packing.
    var manifest = temporary.path_join("custom_sfx_manifest.txt")
    var manifest_writer = FileAccess.open(manifest, FileAccess.WRITE)
    for audio_name in custom_audio_names:
        manifest_writer.store_line(audio_name)
    manifest_writer.close()
    if packer.add_file("res://ErrorRelics/audio/custom_sfx_manifest.txt", manifest) != OK:
        quit(1)
        return
    if packer.flush() != OK:
        quit(1)
        return
    print("ERROR_ASSETS_PACKED: " + output)
    quit(0)

func collect(directory: String, files: Array[String]):
    for file in DirAccess.get_files_at(directory):
        files.append(directory.path_join(file))
    for child in DirAccess.get_directories_at(directory):
        collect(directory.path_join(child), files)
