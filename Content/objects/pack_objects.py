from asyncio.windows_events import NULL
import os
import json
from PIL import Image

def next_power_of_two(n):
    return 1 << (n - 1).bit_length()

class Node:
    def __init__(self, x, y, w, h):
        self.x, self.y, self.w, self.h = x, y, w, h
        self.used = False
        self.down = None
        self.right = None

    def insert(self, w, h):
        if self.used:
            return self.right.insert(w, h) or self.down.insert(w, h)
        elif w <= self.w and h <= self.h:
            self.used = True
            self.down = Node(self.x, self.y + h, self.w, self.h - h)
            self.right = Node(self.x + w, self.y, self.w - w, h)
            return self
        else:
            return None

def pack_images(images):
    total_area = sum(w * h for _, img in images for w, h in [img.size])
    size = next_power_of_two(int(total_area ** 0.5))

    while True:
        root = Node(0, 0, size, size)
        placements = []
        failed = False

        for name, img in images:
            node = root.insert(*img.size)
            if node:
                placements.append((name, img, node.x, node.y))
            else:
                failed = True
                break

        if not failed:
            return size, placements
        size *= 2

def main():
    # load existing objects.json if present
    old_data = {}
    if os.path.exists('objects.json'):
        with open('objects.json', 'r') as f:
            try:
                for obj in json.load(f):
                    old_data[obj["name"]] = obj
            except Exception:
                pass
            
    glow_maps = {}
    images = []
    for fname in os.listdir('raw'):
        if fname.lower().endswith('.png') and not fname.lower().endswith('_glow.png'):
            path = os.path.join('raw', fname)
            img = Image.open(path).convert('RGBA')
            images.append((fname[:-4], img))
            glow_map_path = os.path.join('raw', fname[:-4] + '_glow.png')
            if os.path.exists(glow_map_path):
                glow_map = Image.open(glow_map_path).convert('RGBA')
                glow_maps[fname[:-4]] = glow_map

    atlas_size, placements = pack_images(images)
    atlas = Image.new('RGBA', (atlas_size, atlas_size), (0, 0, 0, 0))
    glow_atlas = Image.new('RGBA', (atlas_size, atlas_size), (0, 0, 0, 0))

    data = []
    for name, img, x, y in placements:
        atlas.paste(img, (x, y))
        glow_map = glow_maps.get(name, False)
        if glow_map:
            glow_atlas.paste(glow_map, (x, y))
        obj = old_data.get(name, {}).copy()
        obj.update({
            "name": name,
            "x": x,
            "y": y,
            "width": img.width,
            "height": img.height,
            "glowing": bool(glow_map)
        })
        data.append(obj)

    atlas.save('objects_tileset.png')
    glow_atlas.save('objects_glowmap.png')

    with open('objects.json', 'w') as f:
        json.dump(data, f, indent=2)

if __name__ == '__main__':
    main()