// Set default enabled state
window.DEBUG = window.DEBUG || false; 

export const DevLogger = {
    enabled: window.DEBUG,

    info(label, value) {
        if (!this.enabled) return;
        console.info(`ℹ️ ${label}`, value);
    },

    warn(label, value) {
        if (!this.enabled) return;
        console.warn(`⚠️ ${label}`, value);
    },

    error(label, value) {
        if (!this.enabled) return;
        console.error(`🔥 ${label}`, value);
    },

    table(title, object) {
        if (!this.enabled) return;
        console.groupCollapsed(`🧾 ${title}`);
        console.table(object);
        console.groupEnd();
    },

    group(title) {
        if (!this.enabled) return;
        console.groupCollapsed(`📦 ${title}`);
    },

    groupEnd() {
        if (!this.enabled) return;
        console.groupEnd();
    }
};

// Default export for backwards compatibility
export default DevLogger;
