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



# DeviantArt Connection
Unfortunatly the DeviantArt Oauth2 login page does not currently work, so this app gets a DeviantArt access token from a cloudflare worker
that handles all the API secrets. The worker is used to generate the access token only. Actual requests for DeviantArt data is fetched 
directly from the DeviantArt API.

As its stands the code assumes the worker supports these URLs (all get requests):

### worker_url/accessToken?appID={your appID here}
gets a DeviantArt access token only if your app has a valid appID

### worker_url/register
generates a new ID for the app. This should be saved for future reuse




# Screenshots
<img src="https://github.com/user-attachments/assets/a5f8bafe-b61c-41aa-89de-6be653371bff" width="300" />
<img src="https://github.com/user-attachments/assets/692124d7-be9f-4f33-8ec4-cc0f3433904b" width="300" />
<img src="https://github.com/user-attachments/assets/bf69053f-061f-4cc6-8030-ae9b85db452d" width="300" />
<img src="https://github.com/user-attachments/assets/7e9b8cd0-3e84-41b5-9fd1-e53a17cad938" width="300" />
<img src="https://github.com/user-attachments/assets/35a3b107-94a5-4850-bae3-7274ccea74ee" width="300" />
<img src="https://github.com/user-attachments/assets/fcf76efe-8c6b-4358-9126-181c795927b2" width="300" />
<img src="https://github.com/user-attachments/assets/6dc7e727-2102-47b5-a953-ed3b9582b44b" width="300" />

