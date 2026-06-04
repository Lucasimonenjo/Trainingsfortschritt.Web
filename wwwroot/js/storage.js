window.idbStorage = {
    get: function (key) {
        return localStorage.getItem(key);
    },

    set: function (key, value) {
        localStorage.setItem(key, JSON.stringify(value));
    },

    remove: function (key) {
        localStorage.removeItem(key);
    },

    clear: function () {
        localStorage.clear();
    }
};