/**
 * PoBabyTouchGc Game JavaScript Functions
 * This file contains all the JS functionality required for the PoBabyTouchGc game
 */

// Function to get element dimensions (width, height)
function getElementDimensions(element) {
    return [element.clientWidth, element.clientHeight];
}

// Audio cache to prevent reloading the same sounds
const audioCache = {};
const audioLoadPromises = {};

// Audio file name mapping based on the actual files in the directories
const audioFiles = {
    "matt": ["Recording.m4a"],
    "kim": ["Recording.m4a"],
    "nick": ["Recording.m4a"]
};

// Function to create a silent audio as fallback
function createSilentAudio() {
    // Create a very short silent audio data URL
    const silentAudioDataUrl = 'data:audio/wav;base64,UklGRigAAABXQVZFZm10IBAAAAABAAEAQB8AAEAfAAABAAgAZGF0YQAAAAA=';
    const audio = new Audio(silentAudioDataUrl);
    return audio;
}

// Function to play sound effect based on person
async function playSound(person) {
    try {
        // Check if person exists in audio files
        if (!audioFiles[person]) {
            console.warn(`No audio files found for person: ${person}`);
            return;
        }
        
        // Use the first (and only) sound file
        const soundIndex = 0; 
        
        // Create a key for caching
        const audioKey = `${person}_${soundIndex}`;
        
        // Check if we're already loading this audio
        if (audioLoadPromises[audioKey]) {
            await audioLoadPromises[audioKey];
        }
        
        // Check if audio is already in cache
        if (!audioCache[audioKey]) {
            // Create a promise for loading this audio
            audioLoadPromises[audioKey] = new Promise((resolve) => {
                // Create new audio object
                const audio = new Audio();
                
                // Set the source path using the correct file names
                const soundFileName = audioFiles[person][soundIndex];
                const soundPath = `sounds/${person}/${soundFileName}`;
                audio.src = soundPath;
                audio.preload = 'auto';
                
                // Add event listeners for better error handling
                audio.addEventListener('error', (e) => {
                    console.warn(`Failed to load audio file: ${soundPath}, using silent fallback`);
                    // Use silent audio as fallback
                    audioCache[audioKey] = createSilentAudio();
                    resolve();
                });
                
                audio.addEventListener('canplaythrough', () => {
                    console.log(`Audio loaded successfully: ${soundPath}`);
                    audioCache[audioKey] = audio;
                    resolve();
                });
                
                // Timeout fallback
                setTimeout(() => {
                    if (!audioCache[audioKey]) {
                        console.warn(`Audio load timeout for: ${soundPath}, using silent fallback`);
                        audioCache[audioKey] = createSilentAudio();
                        resolve();
                    }
                }, 3000); // 3 second timeout
            });
            
            await audioLoadPromises[audioKey];
        }
        
        // Play the sound (clone it for overlapping sounds)
        const cachedAudio = audioCache[audioKey];
        if (cachedAudio) {
            const audioToPlay = cachedAudio.cloneNode();
            audioToPlay.volume = 0.7; // Set volume to 70%
            
            // Return the play promise but handle errors gracefully
            return audioToPlay.play().catch(err => {
                console.log(`Skipping audio playback (this is normal): ${err.message}`);
                // Don't throw the error, just log it
            });
        }
    } catch (err) {
        console.log(`Audio system: ${err.message} (continuing without audio)`);
    }
}

// Function to preload all audio files
function preloadAudio() {
    console.log('Preloading audio files...');
    Object.keys(audioFiles).forEach(person => {
        playSound(person).catch(() => {
            // Ignore preload errors
        });
    });
}

// Call preload when the page loads, but make it optional
document.addEventListener('DOMContentLoaded', () => {
    try {
        preloadAudio();
    } catch (err) {
        console.log('Audio preload skipped:', err.message);
    }
});
