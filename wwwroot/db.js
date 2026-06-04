const DB_NAME = "trainings-db";
const DB_VERSION = 1;

let db;

function openDB() {
    return new Promise((resolve, reject) => {
        const request = indexedDB.open(DB_NAME, DB_VERSION);

        request.onupgradeneeded = (event) => {
            const db = event.target.result;

            if (!db.objectStoreNames.contains("exercises")) {
                db.createObjectStore("exercises", { keyPath: "id" });
            }

            if (!db.objectStoreNames.contains("sets")) {
                const store = db.createObjectStore("sets", { keyPath: "id" });
                store.createIndex("exerciseId", "ExerciseId", { unique: false });
                store.createIndex("date", "Date", { unique: false });
            }

            if (!db.objectStoreNames.contains("goals")) {
                db.createObjectStore("goals", { keyPath: "id" });
            }
        };

        request.onsuccess = () => {
            db = request.result;
            resolve(db);
        };

        request.onerror = () => reject(request.error);
    });
}

async function getStore(name, mode) {
    if (!db) await openDB();
    const tx = db.transaction(name, mode);
    return tx.objectStore(name);
}

window.sqlDb = {

    async getAll(store) {
        const s = await getStore(store, "readonly");
        return new Promise(resolve => {
            const req = s.getAll();
            req.onsuccess = () => resolve(req.result);
        });
    },

    async put(store, value) {
        const s = await getStore(store, "readwrite");
        return new Promise(resolve => {
            s.put(value);
            resolve();
        });
    },

    async delete(store, id) {
        const s = await getStore(store, "readwrite");
        return new Promise(resolve => {
            s.delete(id);
            resolve();
        });
    },

    async clear(store) {
        const s = await getStore(store, "readwrite");
        return new Promise(resolve => {
            s.clear();
            resolve();
        });
    }
};