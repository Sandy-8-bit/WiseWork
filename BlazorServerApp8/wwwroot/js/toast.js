function showToast(message, type) {
    const toast = document.createElement("div");
    toast.className = `toast ${type}`;
    toast.innerText = message;

    // Ensure the toast is visible
    toast.style.display = "block";

    // Common styling for the toast element
    toast.style.position = "fixed";
    toast.style.top = "20px";      // Position at the top
    toast.style.right = "20px";    // Position at the right
    toast.style.padding = "15px 10px";
    toast.style.borderRadius = "4px";
    toast.style.boxShadow = "0px 4px 8px rgba(0, 0, 0, 0.2)";
    toast.style.fontSize = "16px";
    toast.style.maxWidth = "500px";
    toast.style.zIndex = "9999";   // Ensure it's on top of other elements
    toast.style.opacity = "1";
    toast.style.transition = "opacity 0.5s ease-out";

    // Apply different colors based on toast type
    if (type === 'success') {
        toast.style.backgroundColor = "#28a745";
    } else if (type === 'error') {
        toast.style.backgroundColor = "#dc3545";
    } else if (type === 'info') {
        toast.style.backgroundColor = "#17a2b8";
    } else {
        toast.style.backgroundColor = "#6c757d";
    }

    toast.style.color = "white";

    // Add toast to the DOM
    document.body.appendChild(toast);

    // Automatically remove the toast after 3 seconds
    setTimeout(() => {
        toast.style.opacity = "0";  // Fade out
        setTimeout(() => {
            toast.remove();         // Remove from DOM after fade-out completes
        }, 500);                    // Match the transition duration
    }, 3000);
}
