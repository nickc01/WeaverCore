# **ViridianCore**
Core mod for all of my mods for Hollow Knight

## **Project details**

### **ViridianCore**

The base core mod

### **ViridianCore.NLayer**

A wrapper for the ViridianCore mod and the NLayer library. Used decoding mp3 audio files.

https://github.com/naudio/NLayer

### **ViridianLink**

A library used to link Hollow Knight and the Unity Editor together. This library allows you to develop your mods in the Unity Editor, then transfer them seamlessly into Hollow Knight

### **ViridianLink.Editor**

This is only loaded if ViridianLink is in the Unity Editor. This is embedded into ViridianLink as a resource

### **ViridianLink.Game**

This is only loaded if ViridianLink is in Hollow Knight. This is embedded into ViridianLink as a resource

### **ResourceEmbedder**

This library is used to embed assemblies into other assemblies as resources. This is used to embed ViridianLink.Editor and ViridianLink.Game into ViridianLink

### **TestMod**

Used to test the functionality of all the mods. This should be set as the startup project, as it builds all the mods