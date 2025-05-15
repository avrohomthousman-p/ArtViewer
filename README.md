# General Information

The DeviantArt website always displays pictures in a collection in the same order. There is no way to change the order of the pictures.
As a result of this, if you want to browse through a large collection, you will end up only seeing the first 50 or so images. You will
never see any of the rest of the collection unless you spend a long time scrolling and waiting for the browser to load more images.
Often the browser will start to slow down as more and more images are loaded into the DOM. As a result of this, you tend to become far
more familiar with the images at the beginning of the collection, then those at the end. This app fixes that issue by displaying all
images from a collection, but in a random order. The likelihood of seeing a given picture will now be equal across all pictures in the
collection, and you get to see everything your favorite artist has.

The app lets you save individual folders you want to view, as well as letting you save someone's entire gallery or collection. Once you 
have saved a folder, you can access it anytime.



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


#Screenshots
<img src="https://github.com/user-attachments/assets/a5f8bafe-b61c-41aa-89de-6be653371bff" width="300" />
<img src="https://github.com/user-attachments/assets/692124d7-be9f-4f33-8ec4-cc0f3433904b" width="300" />
<img src="https://github.com/user-attachments/assets/bf69053f-061f-4cc6-8030-ae9b85db452d" width="300" />
<img src="https://github.com/user-attachments/assets/7e9b8cd0-3e84-41b5-9fd1-e53a17cad938" width="300" />
<img src="https://github.com/user-attachments/assets/35a3b107-94a5-4850-bae3-7274ccea74ee" width="300" />
<img src="https://github.com/user-attachments/assets/fcf76efe-8c6b-4358-9126-181c795927b2" width="300" />
<img src="https://github.com/user-attachments/assets/6dc7e727-2102-47b5-a953-ed3b9582b44b" width="300" />

