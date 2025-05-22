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

// Audio file name mapping based on the actual files in the directories
const audioFiles = {
    "matt": ["Recording.m4a"],
    "kim": ["Recording.m4a"],
    "nick": ["Recording.m4a"]
};

// Function to play sound effect based on person
function playSound(person) {
    try {
        // Use the first (and only) sound file
        const soundIndex = 0; 
        
        // Create a key for caching
        const audioKey = `${person}_${soundIndex}`;
        
        // Check if audio is already in cache
        if (!audioCache[audioKey]) {
            // Create new audio object
            const audio = new Audio();
            
            // Set the source path using the correct file names
            const soundFileName = audioFiles[person][soundIndex];
            const soundPath = `sounds/${person}/${soundFileName}`;
            audio.src = soundPath;
            
            // Store in cache
            audioCache[audioKey] = audio;
            
            // Log for debugging
            console.log(`Created audio for ${soundPath}`);
        }
        
        // Play the sound (clone it for overlapping sounds)
        const audioToPlay = audioCache[audioKey].cloneNode();
        audioToPlay.volume = 0.7; // Set volume to 70%
        
        // Return the play promise but handle errors
        return audioToPlay.play().catch(err => {
            console.warn(`Error playing sound: ${err.message}`);
        });
    } catch (err) {
        console.error(`Error in playSound function: ${err.message}`);
    }
}
