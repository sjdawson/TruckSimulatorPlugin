# TruckSimulatorPlugin for SimHub

This plugin for [SimHub](https://www.simhubdash.com/) adds multiple new properties, events and actions for the truck simulator games, Euro Truck Simulator 2 and American Truck Simulator.

## Properties

`[TruckSimulatorPlugin.Damage.WearAverage]`

The calculated average of all damage (Cabin, Chassis, Engine, Trailer, Transmission and Wheels) as a percentage. Useful to see an overall state of current damage.

`[TruckSimulatorPlugin.Damage.WearWarning]`

Flips from false to true when `WearAverage` reaches a specified value (currently 5%). Can be used to turn a dashboard light on to indicate that you need to visit a service station. Handy for keeping the costs of damage repair at a maintainable level.

> Roadmap: The % at which `WearWarning` activates will be user configurable

`[TruckSimulatorPlugin.Dash.DisplayUnitMetric]`

Bind a key to the related action `TruckSimulatorPlugin.SwitchDisplayUnit`, and this becomes a condition that allows you to flip between metric and imperial units on the fly. Great for when you're switching from the UK into mainland Europe, and want to have your dashboard showing the local units.

`[TruckSimulatorPlugin.Drivetrain.EcoRange]`

Each of the trucks has a range indicated on the RPM gauge, that lets you know if you're in the econimical range. This flips to true when that is the case.

> Roadmap: At the moment, this may only match a single truck. It needs checking with the other trucks in game.

`[TruckSimulatorPlugin.Job.OverSpeedLimit]`

A simple flag that moves to true when you're over the speed limit. The speed limit in this case is considered to be the current speed limit of the road, plus one. This ensures that there is a small margin available, so that speeds that "match" aren't considered over the limit.

> Roadmap: The margin that's added to the current limit will be user configurable.

`[TruckSimulatorPlugin.Job.OverSpeedLimitPercentage]`

As you rise above the speed limit, this moves from 0-1 representing a percentage of how far over the limit you are. By default, this considers current speed limit of the road plus 2, to be 100% over the speed limit. This can be used to make smooth transitions of elements to represent a warning of being above the speed limit, and into the area that a fine is likely.

> Roadmap: The amount to add to the current speed limit will be user configurable.

`[TruckSimulatorPlugin.Lights.HazardWarningActive]`

A shortcut method that checks to see if both left and right turn signals are "active". This is only ever the case when the hazard warning lights are showing.

## Actions

`TruckSimulatorPlugin.SwitchDisplayUnit`

Use this event to switch the related property `TruckSimulatorPlugin.Dash.DisplayUnitMetric` between true and false.