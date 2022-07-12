from model.game import Game
from debugging import Color
from model.item import ShieldPotions, Weapon
from model.order import Order
from model.unit_order import UnitOrder
from model.constants import Constants
from model.vec2 import Vec2
from model.action_order import ActionOrder
from typing import Optional
from debug_interface import DebugInterface


class MyStrategy:
    def __init__(self, constants: Constants):
        self.const = constants
        pass
    def get_order(self, game: Game, debug_interface: Optional[DebugInterface]) -> Order:
        orders = {}
        r = game.zone.next_radius
        center = game.zone.next_center
        Enemy = False
        for unit in game.units:	
            if unit.player_id == game.my_id:
                UnitMe = unit
        for unit in game.units:	    
            if unit.player_id != game.my_id:
                if Enemy and (((UnitMe.position.x - UnitEnemy.position.x)**2+(UnitMe.position.y - UnitEnemy.position.y)**2)**0.5 < ((UnitMe.position.x - unit.position.x)**2+(UnitMe.position.y - unit.position.y)**2)**0.5):
                    continue    
                Enemy = True
                UnitEnemy = unit
        hasLoot = False
        for loot in game.loot:
            if hasLoot and (((UnitMe.position.x - NearLoot.position.x)**2+(UnitMe.position.y - NearLoot.position.y)**2)**0.5 < ((UnitMe.position.x - loot.position.x)**2+(UnitMe.position.y - loot.position.y)**2)**0.5):
                continue
            if not isinstance(loot.item, ShieldPotions):
                continue
            if ((center.x - loot.position.x)**2+(center.y - loot.position.y)**2)**0.5 >= r:
                continue
            NearLoot = loot
            hasLoot = True
        if Enemy:
            actionMe = ActionOrder.Aim(True)
            if UnitMe.health < self.const.unit_health:
                if hasLoot:
                    UnitPosNext = Vec2(NearLoot.position.x - UnitMe.position.x, NearLoot.position.y - UnitMe.position.y)
                    actionMe = ActionOrder.Pickup(NearLoot.id)
                else:
                    UnitPosNext = Vec2(UnitMe.position.x - UnitEnemy.position.x, UnitMe.position.y - UnitEnemy.position.y)
            else:
                UnitPosNext = Vec2(UnitEnemy.position.x - UnitMe.position.x, UnitEnemy.position.y - UnitMe.position.y)
            UnitDir = Vec2(UnitEnemy.position.x - UnitMe.position.x, UnitEnemy.position.y - UnitMe.position.y)
        else:
            if hasLoot:
                UnitPosNext = Vec2(NearLoot.position.x - UnitMe.position.x, NearLoot.position.y - UnitMe.position.y)
                UnitDir = Vec2(NearLoot.position.x - UnitMe.position.x, NearLoot.position.y - UnitMe.position.y)
                actionMe = ActionOrder.Pickup(NearLoot.id)
            else:
                actionMe = ActionOrder.Aim(False)
                if UnitMe.health < self.const.unit_health:
                    havesound = False
                    for unit in game.sounds:
                        if havesound and (((UnitMe.position.x - MakeSound.position.x)**2+(UnitMe.position.y - MakeSound.position.y)**2)**0.5 < ((UnitMe.position.x - unit.position.x)**2+(UnitMe.position.y - unit.position.y)**2)**0.5):
                            continue
                        havesound = True
                        MakeSound = unit
                    if havesound:
                        UnitPosNext = Vec2(MakeSound.position.x - UnitMe.position.x, MakeSound.position.y - UnitMe.position.y)
                        UnitDir = Vec2(MakeSound.position.x - UnitMe.position.x, MakeSound.position.y - UnitMe.position.y)
                    else:
                        UnitPosNext = Vec2(center.x-UnitMe.position.x, center.y-UnitMe.position.y)
                        UnitDir = Vec2(-UnitMe.direction.y, UnitMe.direction.x) 
                    debug_interface.add_placed_text(UnitMe.position, str(True), Vec2(0,0), 1, Color(1,0,1,1))
                else:
                    UnitPosNext = Vec2(center.x-UnitMe.position.x, center.y-UnitMe.position.y)
                    UnitDir = Vec2(-UnitMe.direction.y, UnitMe.direction.x)            
        if UnitMe.health < self.const.unit_health and UnitMe.shield_potions:
            actionMe = ActionOrder.UseShieldPotion()
        orders[UnitMe.id] = UnitOrder(
            UnitPosNext,
            UnitDir,
            actionMe)
        return Order(orders)
    def debug_update(self, displayed_tick: int, debug_interface: DebugInterface):
        pass
    def finish(self):
        pass
