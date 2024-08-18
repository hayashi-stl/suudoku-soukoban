import bpy
import bpy_extras
import pathlib

path = f"{bpy.path.abspath('//')}/../Object/Coin/Coin Outline"

frame_start = bpy.context.scene.frame_start
frame_end = bpy.context.scene.frame_end + 1
num_frames = frame_end - frame_start
original_frame = bpy.context.scene.frame_current
original_render_path = bpy.context.scene.render.filepath

tmp_file = f"{path}_tmp.png"
try:
    bpy.context.scene.render.filepath = tmp_file
    image = bpy.data.images.new("image", 12, 34)
    image.source = "FILE"
    
    size = [bpy.context.scene.render.resolution_x, bpy.context.scene.render.resolution_y]
    stride = size[0] * size[1] * 4
    output = bpy.data.images.new("output", size[0], size[1] * num_frames, alpha=True)
    for i in range(num_frames):
        frame = frame_start + i
        bpy.context.scene.frame_current = frame
        bpy.ops.render.render(write_still=True)
        image.filepath = tmp_file
        index = num_frames - 1 - i
        output.pixels[stride * index : stride * index + stride] = image.pixels
        
    output.save(filepath=f"{path}.png")
finally:
    bpy.context.scene.render.filepath = original_render_path
    pathlib.Path(tmp_file).unlink(missing_ok=True)
    bpy.context.scene.frame_current = original_frame
    for im in [output, image]:
        if im:
            bpy.data.images.remove(im)
    pass