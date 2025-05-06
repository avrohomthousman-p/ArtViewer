# General Information

The DeviantArt website always displays pictures in a collection in the same order. There is no way to change the order of the pictures.
As a result of this, if you want to browse through a large collection, you will end up only seeing the first 50 or so images. You will
never see any of the rest of the collection unless you spend a long time scrolling and waiting for the browser to load more images.
Often the browser will start to slow down as more and more images are loaded into the DOM. As a result of this, you tend to become far
more familiar with the images at the beginning of the collection, then those at the end. This app fixes that issue by displaying all
images from a collection, but in a random order. The likelihood of seeing a given picture will now be equal across all pictures in the
collection.

It should be noted that currently this app is hardcoded to only view the collection of one very specific user. Hopefully this will be
changed at some point in the future.


# Installation
It is important to understand how this app manages its secrets. The app has a `Secrets.cs` file (ArtViewer\ArtViewer\Secrets.cs).
When the app is being built, before any code is compiled, a special script is called. This script (found in 
"ArtViewer\GenerateSecrets.csx") will search the project root for a `.env` file containing a `client_id` and a `client_secret`, 
which are both necessary for connecting to the DeviantArt API. The script reads the values from the `.env` file and uses them to 
replace the dummy values in `ArtViewer\ArtViewer\Secrets.cs`, so that real data will be available at runtime. This process will
completely overwrite the contents of the secrets file, and it will contain actual secrets in plain text. As a result of this system,
there are a few things that need to be done (in any order) before building the app for the first time. 

1) run this command:
`git update-index --assume-unchanged ArtViewer\Secrets.cs`
This ensures that when the secrets file is overwritten, it is not registered as a change and does not get pushed.

2) Make a DeviantArt account for yourself and [register your app](https://www.deviantart.com/developers/apps). They will provide you
with a client_id and client_secret. Then put those values into a `.env` file in the project's root directory like this:
```
client_id=your_client_id_here
client_secret=your_secret_here
```
Do not have trailing or leading whitespace on any line or at the beginning or end of the file. It might cause the script to fail.

3) You will need to install the scripting software that is required by the above mentioned script. To do this, navigate to the
project root directory and run this command in a terminal
`dotnet tool restore`
