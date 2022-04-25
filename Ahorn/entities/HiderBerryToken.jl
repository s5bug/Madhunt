module MadhuntHiderBerryToken
using ..Ahorn, Maple

@mapdef Entity "Madhunt/HiderBerryToken" HiderBerryToken(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "Hider Berry Token (Madhunt)" => Ahorn.EntityPlacement(
        HiderBerryToken
    )
)

const sprite = "collectables/strawberry/normal00.png"

function Ahorn.selection(entity::HiderBerryToken)
    x, y = Ahorn.position(entity)
    return Ahorn.getSpriteRectangle(sprite, x, y)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::HiderBerryToken, room::Maple.Room) = Ahorn.drawSprite(ctx, sprite, 0, 0)

end