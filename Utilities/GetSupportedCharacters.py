""" 
This script prints out a <CharacterRegions> xml element that contains efficiently calculated regions for
all of the font's supported unicode characters. Pass the font file (not the spritefont xml) as an argument.
This script requires FontTools. Install using: pip install fonttools 
"""

""" The amount of characters that can be missing in a sequence to still put them in one region """
leeway = 0

from fontTools.ttLib import TTFont
from sys import argv


def print_region(start, end):
    print(f"  <CharacterRegion>")
    print(f"    <Start>&#{start};</Start>")
    print(f"    <End>&#{end};</End>")
    print(f"  </CharacterRegion>")


# get all supported characters and sort them
ttf = TTFont(argv[1])
chars = list(set(y[0] for x in ttf["cmap"].tables for y in x.cmap.items()))
chars.sort()
ttf.close()

# split them into regions based on the leeway
start = -1
last = 0
total = 0
print("<CharacterRegions>")
for char in chars:
    if char - last > leeway + 1:
        if start != -1:
            print_region(start, last)
            total += last - start + 1
        start = char
    last = char
# print the remaining region
print_region(start, last)
total += last - start + 1
print("</CharacterRegions>")

print(f"The font contains {len(chars)} characters")
print(f"The spritefont will contain {total} characters")