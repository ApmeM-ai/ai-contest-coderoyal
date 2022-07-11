from model.game import Game
from model.order import Order
from model.unit_order import UnitOrder
from model.constants import Constants
from model.vec2 import Vec2
from model.action_order import ActionOrder
from typing import Optional
from debug_interface import DebugInterface


class MyStrategy:
    def __init__(self, constants: Constants):
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
        if Enemy:
            Aim = True
            UnitPosNext = Vec2(UnitEnemy.position.x - UnitMe.position.x, UnitEnemy.position.y - UnitMe.position.y)
            UnitDir = Vec2(UnitEnemy.position.x - UnitMe.position.x, UnitEnemy.position.y - UnitMe.position.y)
        else:
            Aim = False
            UnitPosNext = Vec2(center.x-UnitMe.position.x, center.y-UnitMe.position.y)
            UnitDir = Vec2(-UnitMe.direction.y, UnitMe.direction.x)
        orders[UnitMe.id] = UnitOrder(
            UnitPosNext,
            UnitDir,
            ActionOrder.Aim(Aim))
        return Order(orders)
    def debug_update(self, displayed_tick: int, debug_interface: DebugInterface):
        pass
    def finish(self):
        pass
