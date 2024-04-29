# Looping Sound Fix

Fixes looping sounds sometimes not ending and playing indefinitely. Clientside.

## What? How? Why? Who? When? Where?

2 of these questions will be answered, the rest is up to you to figure out. I believe in you.

### What?

In some cases when the game starts a looping sound, it doesn't ever send the "stop" signal, so the sound sticks around indefinitely.

### How?

The change this mod does is pretty simple: When an object is removed by the game, all active sounds associated with that object are also removed.

This does **not** stop all cases of looping sounds going on for longer than they should, but it fixes them looping indefinitely, the usual worst case-scenario is they last until a stage transition.