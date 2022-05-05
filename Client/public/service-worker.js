// Establish a cache name
const cacheName = 'SpeedServiceWorker_v1.0.0.1';

self.addEventListener('install', (event) => {
	event.waitUntil(caches.open(cacheName));
});

self.addEventListener('activate', event => {
// Remove old caches
	event.waitUntil(
		(async () => {
			const keys = await caches.keys();
			return keys.map(async (cache) => {
				if(cache !== cacheName) {
					console.log('Service Worker: Removing old cache: '+cache);
					return await caches.delete(cache);
				}
			})
		})()
	)
})

self.addEventListener('fetch', async (event) => {
	// Is this a request for an image?
	if (event.request.destination === 'image') {
		// Open the cache
		event.respondWith(caches.open(cacheName).then((cache) => {
			// Respond with the image from the cache or from the network
			return cache.match(event.request).then((cachedResponse) => {
				return cachedResponse || fetch(event.request.url).then((fetchedResponse) => {
					// Add the network response to the cache for future visits.
					// Note: we need to make a copy of the response to save it in
					// the cache and use the original as the request response.
					cache.put(event.request, fetchedResponse.clone());

					// Return the network response
					return fetchedResponse;
				});
			});
		}));
	} else {
		return;
	}
});

// This allows the web app to trigger skipWaiting via
// registration.waiting.postMessage({type: 'SKIP_WAITING'})
self.addEventListener('message', (event) => {
	if (event.data && event.data.type === 'SKIP_WAITING') {
		self.skipWaiting()
	}
})
