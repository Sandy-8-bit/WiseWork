window.getLocation = function () {
    return new Promise((resolve, reject) => {
        if (navigator.geolocation) {
            navigator.geolocation.getCurrentPosition(
                (position) => {
                    resolve({
                        latitude: position.coords.latitude,
                        longitude: position.coords.longitude
                    });
                },
                (error) => {
                    reject("Unable to retrieve your location");
                }
            );
        } else {
            reject("Geolocation is not supported by this browser.");
        }
    });
};
