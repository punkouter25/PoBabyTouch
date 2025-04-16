/**
 * PoBabyTouch Game JavaScript Functions
 * This file contains all the JS functionality required for the PoBabyTouch game
 */

// Audio collections for each person
const audioFiles = {
    matt: [
        'sounds/matt/sound1.mp3',
        'sounds/matt/sound2.mp3',
        'sounds/matt/sound3.mp3',
        'sounds/matt/sound4.mp3',
        'sounds/matt/sound5.mp3'
    ],
    nick: [
        'sounds/nick/sound1.mp3',
        'sounds/nick/sound2.mp3',
        'sounds/nick/sound3.mp3',
        'sounds/nick/sound4.mp3',
        'sounds/nick/sound5.mp3'
    ],
    kim: [
        'sounds/kim/sound1.mp3',
        'sounds/kim/sound2.mp3',
        'sounds/kim/sound3.mp3',
        'sounds/kim/sound4.mp3',
        'sounds/kim/sound5.mp3'
    ]
};

// Audio objects cache for preloading
const audioCache = {
    matt: [],
    nick: [],
    kim: []
};

// Preload all audio files
function preloadAudio() {
    console.log('Preloading audio files...');
    
    for (const person in audioFiles) {
        const files = audioFiles[person];
        
        for (let i = 0; i < files.length; i++) {
            const audio = new Audio();
            audio.src = files[i];
            audio.load(); // Start loading the audio file
            audioCache[person].push(audio);
            
            // Log when each file is loaded
            audio.oncanplaythrough = () => {
                console.log(`Loaded audio: ${files[i]}`);
            };
            
            // Log any loading errors
            audio.onerror = (e) => {
                console.error(`Error loading audio: ${files[i]}`, e);
            };
        }
    }
    
    console.log('Audio preloading initiated');
}

/**
 * Play a random sound for the specified person
 * @param {string} person - The person identifier (matt, nick, or kim)
 * @returns {Promise} A promise that resolves when the audio starts playing
 */
window.playSound = function(person) {
    return new Promise((resolve, reject) => {
        try {
            if (!audioCache[person] || audioCache[person].length === 0) {
                console.warn(`No audio cache found for ${person}, attempting to load on demand`);
                // If audio isn't preloaded, create a new audio element
                const randomIndex = Math.floor(Math.random() * audioFiles[person].length);
                const audio = new Audio(audioFiles[person][randomIndex]);
                
                audio.oncanplaythrough = () => {
                    audio.play();
                    resolve();
                };
                
                audio.onerror = (e) => {
                    console.error(`Error playing audio for ${person}:`, e);
                    reject(e);
                };
                
                return;
            }
            
            // Get a random audio file from the cache
            const randomIndex = Math.floor(Math.random() * audioCache[person].length);
            const audio = audioCache[person][randomIndex];
            
            // Reset the audio to the beginning if it was played before
            audio.currentTime = 0;
            
            // Play the sound
            audio.play()
                .then(() => {
                    console.log(`Playing sound for ${person}`);
                    resolve();
                })
                .catch(e => {
                    console.error(`Error playing audio for ${person}:`, e);
                    reject(e);
                });
        } catch (e) {
            console.error(`Exception playing sound for ${person}:`, e);
            reject(e);
        }
    });
};

/**
 * Get the dimensions of an element
 * @param {ElementReference} element - The element to measure
 * @returns {number[]} An array containing the width and height
 */
window.getElementDimensions = function(element) {
    if (!element) {
        console.error('Element reference is null');
        return [0, 0];
    }
    
    const width = element.clientWidth;
    const height = element.clientHeight;
    
    console.log(`Element dimensions: ${width}x${height}`);
    return [width, height];
};

/**
 * Check the internet connectivity
 * @returns {Promise<boolean>} A promise that resolves to true if online, false otherwise
 */
window.checkInternetConnectivity = function() {
    return new Promise((resolve) => {
        // Check if the browser reports being online
        if (navigator.onLine) {
            // Double-check by trying to fetch a small resource from a reliable server
            fetch('https://www.microsoft.com/favicon.ico', { 
                mode: 'no-cors',
                cache: 'no-store'
            })
            .then(() => {
                console.log('Internet connectivity: ONLINE');
                resolve(true);
            })
            .catch(() => {
                console.log('Internet connectivity: OFFLINE (fetch failed)');
                resolve(false);
            });
        } else {
            console.log('Internet connectivity: OFFLINE (navigator.onLine)');
            resolve(false);
        }
    });
};

// Initialize preloading when the page loads
document.addEventListener('DOMContentLoaded', () => {
    console.log('DOM loaded, initializing game audio system');
    preloadAudio();
});