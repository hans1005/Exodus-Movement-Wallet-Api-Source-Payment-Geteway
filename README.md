# Exodus-Movement-Wallet-Api-Source-Payment-Geteway

This is a service to provide rich API for manage tokens on Zcoin Exodus Protocol.

![Exodus_Logo-removebg-preview](https://user-images.githubusercontent.com/106811566/171851878-bf94716c-f545-4249-911a-ec535dc0a60a.png)


## Development

### Requirements

- .NET Core 2.1

### Build

```sh
dotnet build src/Ztm.sln
```

### Start Required Services

You need to install [Docker Compose](https://do6cs.65/) first then run:

```sh
docker-compose up -d
```
![image](https://user-images.githubusercontent.com/106811566/171851917-bd154f89-2e32-485c-bf92-2ef96bb784ac.png)

### Migrate Database Schemas

Change directory to `src/Ztm.Data.Entity.Postgres` then run:

```sh
ZTM_MAIN_DATABASE="Host=127.0.0.1;Database=postgres;Username=postgres" dotnet ef database update
```

### Start Web API

```sh
dotnet run -p src/Ztm.WebApi
```

Things you may want to cover:

* Ruby version

* System dependencies

* Configuration

* Database creation

* Database initialization

* How to run the test suite

* Services (job queues, cache servers, search engines, etc.)

* Deployment instructions

* ...

## Installation - Unreal side.

* Within your C++ project, create "Plugins" folder if it does not already exist.
* Copy or SymLink "ExodusImport" folder there. 
* Reload the project.
* Go to "Plugins" menu in unreal editor, find "ExodusImport" under "Other" category, and enable it. The project will be restarted.
* Recompile the project if necessary.
* Once successfully installed, "Import" command will be available via button bar above scene view.

Please disregard buttons that are not marked "Import", if such buttons are present. Those are test cases.

## Usage

The exporter is accessible either through rightclick within hierarchy view in Unity, OR through "Migrate to UE4" within main menu of unity.
Following options are available:

* *Export current object* - will export current object only with minimal information about the rest of the scene.
* *Export selected objects* - will export selected objects within current scene
* *Export current scene* - will attempt to export current scene and all objects in it.
* *Export current project* - the plugin will attempt to enumerate all resources within the project and export all of them, including scenes.

Once you selected desired option, you'll be prompted to pick up an empty location for the "project" fuke and exported data. It is a good idea to select an empty folder without anything else in it.
The exported data constists of one "master" file in json format and a folder with similar name. Once project export starts, the plugin will copy and convert relevant data into the destination folder.

On unreal side simply find "Import" button in the tool bar, and select the \*.json file you exported with it. 

In situation where you exported current scene only, the scene will be exported into current unreal scene and merged with it.
However, if your scene contains terrain, it will be imported as a scene file, which will be located at /Import/<UnityProjectName>/<SceneName>.
When this happens, you'll see a warning, and request to wait till shader compilation is done. Wait till shaders finish compiling, then head to the location of the scene file and open it.

In situation where you exported multiple scenes, the exporter/importer will try to store them within paths similar to those used in unity. Meaning the imported scene content should be under /Import/<UnityPorjectName>/
and then in whatever unity folder path you used.

## What's supported and limitations

The plugin will rebuild current scene or scenes, will convert static meshes, and will *attempt* to convert terrain, landscapes, and skeletal meshes to unreal format. The plugin will also attempt to recreate materials.

Currently following limitations are in place:

* Only "Standard" and "Standard (Specular setup)" materials are currently supported. However, all parameters of those materials should be supported.
* Static meshes are supported. UV coordinates, vertex positions will be converted to unreal format.
* Light and their parameter conversion is supported.
* Reflection probes are supported.
* The plugin will attempt to transfer flags such as being static, having specific shadowcasting type, etc.
* Surface shaders and custom shaders are **not supported** and cannot be converted. The plugin will attempt to harvest their properties, but if those properties do not match properties of standard material, The material will likely appear blacko n unreal side.
* All texture formats that are not directly supported by unreal engine will be converted to png. Due to the way conversion is handled, minor data loss may occur in the process. As a result, please replace those automatically-converted textures when you can.
* Reflection cubemaps used by reflection probes will be converted by similar process and may have minor dataloss. Consider replacing them when you have opportunity.
* Prefabs are not currently converted into blueprints.
* Empty GameObject nodes that are used for "bookkeeping" purposes will be converted into unreal 4 folders within scene view.
* Due to differences of handling landscapes, 1:1 identical transfer is impossible. Maps used by terrain system will be resampled upon import, and trees will lose custom tint. The plugin will attempt to preserve grass density, but grass clump placement will differ.
* The skinned mesh/character conversion is only partially supported, and upon import character may end up being split into several objects. The plugin will attempt to convert animation clips used by the controller, but will not recreate statemachine. Artifacts are possible in converted character.
* Additional limitations may apply.

Additionally, the file format used for transferring the project is subject to change and should not be used for long term data storage or backup. 

## Additional contact information:

Bugs can be reported via github project page.
Additionally I can be reached through email address: neginfinity000<at>gmail.com

## Contacts

The preferred method for submitting bugs is github. However, I can also be reached via an email address:
