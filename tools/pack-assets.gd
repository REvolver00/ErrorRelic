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
        elif file.ends_with(".json"):
            if packer.add_file("res://" + relative, file) != OK:
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
