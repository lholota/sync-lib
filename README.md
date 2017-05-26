# LiteDB Sync Library

This library allows you to sync the LiteDB database objects between various devices. By default it does so using user's storage in common cloud storage services like OneDrive, DropBox and others. Why?
- Cost - if you are developing and open source app, you are usually developing without gaining any non-profit, hence you don't want to spend money on running custom services just to sync the data between devices.
- Privacy - this way the data is stored on user's account and you as the author of the app don't have any access to the data therefore your users will be more inclined to use the app even for sensitive data.

## How can I use it?

The library provides wrappers around the standard LiteDB objects (LiteRepository only at first). Which intercepts the data manipulation calls to add metadata for sync. Therefore there is no change of logic required in your code to use the sync library. You only need to reference a NuGet package (netstandard)

TBA nuget link

and configure the sync options, check the sample below:

TBA code sample

Note: you need to get your own appId, secrets etc. from each provider. See the Providers section below, there's a link to a guide for each of the providers.

### Onboarding the user

Before saving the data to the cloud storage, you need the user to authorize your app. This is done by opening a web browser in the app where the user logs into their account and authorizes the app.

## How does it work?
TBA

### Remote structure
The data is saved in an application directory which is created when an application is registered by the provider. The logic itself is very similar to Git's commit and HEAD:
- Each transaction is serialized into a separate json file which is located in the Transactions directory. The file name is a random guid.
- The file contains a parent transaction name (i.e. which transaction it's based on)
- In the root of the appdata directory, there's a Head.json file which points to the latest transaction.

### Advanced conflict resolution
TBA

### Nearly real-time sync
TBA - LiteDbSyncService interface description

### Providers
- One Drive
- Google Drive
- DropBox
