from model.game import Game
from debugging import Color
from model.item import Ammo, ShieldPotions, Weapon
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
        me = self.findMe(game)
        for unit in game.units:
            if unit.player_id != game.my_id:
                continue
            orders[unit.id] = UnitOrder(
                self.nextPoint(game, me),
                self.nextDir(game,me),
                self.nextAction(game,me))      
        return Order(orders)
    def debug_update(self, displayed_tick: int, debug_interface: DebugInterface):
        pass
    def finish(self):
        pass
    def distanceM(self, point1, point2):
        dist = ((point1.position.x - point2.position.x)**2+(point1.position.y - point2.position.y)**2)**0.5
        return dist
    def hasEnemy(self, myUnit, enemyUnit):
        if myUnit.health < self.const.unit_health:
            UnitPosNext = Vec2(myUnit.position.x - myUnit.position.x, myUnit.position.y - myUnit.position.y)
        else:
            UnitPosNext = Vec2(enemyUnit.position.x - myUnit.position.x, enemyUnit.position.y - myUnit.position.y)
        UnitDir = Vec2(enemyUnit.position.x - myUnit.position.x, enemyUnit.position.y - myUnit.position.y)
        return UnitOrder(
            UnitPosNext,
            UnitDir,
            ActionOrder.Aim(True))
    def nextPoint(self, game, myUnit):
        nearestEnemy = self.findNearestEnemy(game,myUnit)
        if myUnit.weapon == 0 or myUnit.weapon == 1 or myUnit.wepon == None:
            point = self.findLoot(game, myUnit, Weapon)
        if nearestEnemy:
            if myUnit.health < self.const.unit_health:
                point = Vec2(myUnit.position.x - nearestEnemy.position.x, myUnit.position.y - nearestEnemy.position.y)
            else:
                point = Vec2(nearestEnemy.position.x - myUnit.position.x, nearestEnemy.position.y - myUnit.position.y)
        else:
            nearestLoot = self.findLoot(game, myUnit, ShieldPotions)
            if nearestLoot:
                point = Vec2(nearestLoot.position.x - myUnit.position.x, nearestLoot.position.y - myUnit.position.y)
            else:
                if myUnit.weapon == 0 or myUnit.weapon == 1:
                    nearestLoot = self.findLoot(game, myUnit, Weapon)
                    if nearestLoot:
                        point = Vec2(nearestLoot.position.x - myUnit.position.x, nearestLoot.position.y - myUnit.position.y)
                    else:
                        point = game.zone.next_center
                else:
                    nearestLoot = self.findLoot(game, myUnit, Ammo)
                    if nearestLoot:
                        point = Vec2(nearestLoot.position.x - myUnit.position.x, nearestLoot.position.y - myUnit.position.y)
                    else:
                        point = game.zone.next_center
        return point
    def nextDir(self, game,myUnit):
        nearestEnemy = self.findNearestEnemy(game,myUnit)
        if nearestEnemy:
            direction = Vec2(nearestEnemy.position.x - myUnit.position.x, nearestEnemy.position.y - myUnit.position.y)
        else:
            direction = Vec2(-myUnit.direction.y, myUnit.direction.x)
        return direction
    def nextAction(self, game,myUnit):
        nearestEnemy = self.findNearestEnemy(game,myUnit)
        if nearestEnemy:
            action = ActionOrder.Aim(True)
        else:
            action = ActionOrder.Aim(False)
        nearestLoot = self.findLoot(game,myUnit,ShieldPotions)
        if nearestLoot and self.distanceM(myUnit,nearestLoot) < self.const.unit_radius:
            action = ActionOrder.Pickup(nearestLoot.id)
        nearestLoot = self.findLoot(game,myUnit,Weapon)
        if nearestLoot and self.distanceM(myUnit,nearestLoot) < self.const.unit_radius:
            action = ActionOrder.Pickup(nearestLoot.id)
        nearestLoot = self.findLoot(game,myUnit,Ammo)
        if nearestLoot and self.distanceM(myUnit,nearestLoot) < self.const.unit_radius:
            action = ActionOrder.Pickup(nearestLoot.id)
        if myUnit.shield < self.const.max_shield and myUnit.shield_potions:
            action = ActionOrder.UseShieldPotion()
        return action
    def findLoot(self, game, myUnit, classLoot):
        r = game.zone.next_radius
        center = game.zone.next_center
        hasLoot = False
        for loot in game.loot:
            if hasLoot and self.distanceM(myUnit,NearLoot) < self.distanceM(myUnit,loot):
                continue
            if not isinstance(loot.item, classLoot):
                continue
            if ((center.x - loot.position.x)**2+(center.y - loot.position.y)**2)**0.5 >= r:
                continue
            if classLoot == Weapon:
                if loot.item.type_index == 0 or loot.item.type_index == 1:
                    continue
            if classLoot == Ammo:
                if loot.item.weapon_type_index == myUnit.weapon:
                    continue
            NearLoot = loot
            hasLoot = True
        if hasLoot:
            return NearLoot
        else:
            return False
    def findMe(self, game):
        for unit in game.units:	
            if unit.player_id == game.my_id:
                me = unit
        return me
    def findNearestEnemy(self, game, myUnit):
        Enemy = False
        for unit in game.units:	    
            if unit.player_id != game.my_id:
                if Enemy and self.distanceM(myUnit, nearestEnemy) < self.distanceM(myUnit, unit):
                    continue    
                Enemy = True
                nearestEnemy = unit
        if Enemy:
            return nearestEnemy
        else:
            return False
