mergeInto(LibraryManager.library, {
    // WebGL keeps persistentDataPath in memory (IDBFS); this writes it to IndexedDB.
    SeweralIdeasConfig_SyncFileSystem: function () {
        if (typeof FS === 'undefined' || !FS.syncfs)
            return;
        FS.syncfs(false, function (err) {
            if (err)
                console.error('Config: could not sync the file system to IndexedDB', err);
        });
    }
});
