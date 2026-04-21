// Smooth transitions for pagination
document.addEventListener('DOMContentLoaded', function() {
    // Add fade-in effect when page loads
    const tableContainer = document.querySelector('.overflow-x-auto');
    if (tableContainer) {
        tableContainer.style.opacity = '0';
        tableContainer.style.transition = 'opacity 0.3s ease-in-out';

        // Fade in after a short delay
        setTimeout(() => {
            tableContainer.style.opacity = '1';
        }, 100);
    }

    // Handle pagination link clicks for smooth transitions
    const paginationLinks = document.querySelectorAll('a[href*="?page="]');
    paginationLinks.forEach(link => {
        link.addEventListener('click', function(e) {
            const table = document.querySelector('.overflow-x-auto');
            if (table) {
                e.preventDefault();

                // Fade out
                table.style.opacity = '0';

                // Navigate after fade out
                setTimeout(() => {
                    window.location.href = this.href;
                }, 300);
            }
        });
    });

    // Add hover effects to pagination buttons
    const pageButtons = document.querySelectorAll('.relative.inline-flex.items-center');
    pageButtons.forEach(button => {
        button.addEventListener('mouseenter', function() {
            this.style.transform = 'translateY(-1px)';
            this.style.transition = 'transform 0.2s ease';
        });

        button.addEventListener('mouseleave', function() {
            this.style.transform = 'translateY(0)';
        });
    });

    // Animate page size selector
    const pageSizeSelect = document.getElementById('pageSize');
    if (pageSizeSelect) {
        pageSizeSelect.addEventListener('change', function() {
            const container = document.querySelector('.overflow-x-auto');
            if (container) {
                container.style.opacity = '0';
                setTimeout(() => {
                    container.style.opacity = '1';
                }, 100);
            }
        });
    }

    // Add loading state for better UX
    function addLoadingState(element) {
        element.disabled = true;
        element.classList.add('opacity-50', 'cursor-not-allowed');

        // Remove loading state after 1 second
        setTimeout(() => {
            element.disabled = false;
            element.classList.remove('opacity-50', 'cursor-not-allowed');
        }, 1000);
    }

    // Apply to all pagination links
    paginationLinks.forEach(link => {
        link.addEventListener('click', function() {
            addLoadingState(this);
        });
    });
});