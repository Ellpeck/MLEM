-- This is a lua script for Aseprite that allows automatically populating a MLEM Data Texture Atlas.
-- To use this script, you need to select a rectangular area that should be your texture region.
-- If you want a custom pivot point, you also need to un-select exactly one pixel, the pivot point.
-- When you then execute the script and input the name of the texture region, it gets appended to the "SpriteName.atlas" file.

-- get the currently selected sprite
local selection = app.activeSprite.selection
if selection == nil then return end
local bounds = selection.bounds

local loc = "loc "..bounds.x.." "..bounds.y.." "..bounds.width.." "..bounds.height.."\n"
local piv = ""

-- find the pivot point, which should be the only de-selected pixel in the selection
for x = bounds.x, bounds.x + bounds.width - 1 do
    for y = bounds.y, bounds.y + bounds.height - 1 do
        if not selection:contains(x, y) then 
            piv = "piv "..x.." "..y.."\n"
            goto foundPivot
        end
    end
end
::foundPivot:: 

-- open the name dialog
local dialog = Dialog("Copy Texture Atlas Data")
    :entry{ id = "name", label = "Region Name", focus = true }
    :button{ id = "ok", text="&OK" }
    :button{ text = "&Cancel" }
dialog:show()

local data = dialog.data
if not data.ok then return end
local name = data.name

-- get the atlas file and write the data to it
local spriteFile = app.activeSprite.filename
local atlas = spriteFile:match("(.+)%..+")..".atlas"
local file = io.open(atlas, "a")
file:write("\n"..name.."\n"..loc..piv)
file:close()