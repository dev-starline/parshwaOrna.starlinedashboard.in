function validateSubCategoryName(form) {
    const nameInput = form.querySelector('#subCategoryName');
    const errorMsg = document.getElementById('nameErrorMsg');

    if (!nameInput.value.trim()) {
        alert("Please enter Name");
        errorMsg.style.display = "block";
        nameInput.focus();
        return false;
    } else {
        errorMsg.style.display = "none";
        return true;
    }
}

function hideNameError() {
    const errorMsg = document.getElementById('nameErrorMsg');
    errorMsg.style.display = "none";
}

function loadSubCategories(categoryId) {
    const subCatDropdown = document.getElementById("SubCategoryId");
    subCatDropdown.innerHTML = '<option value="">Loading...</option>';

    if (!categoryId) {
        subCatDropdown.innerHTML = '<option value="">Select Sub-Category</option>';
        return;
    }

    fetch(`/Jewellery/GetSubCategories?categoryId=${categoryId}`)
        .then(response => response.json())
        .then(data => {
            let options = '<option value="">Select Sub-Category</option>';
            data.forEach(function (sub) {
                options += `<option value="${sub.id}">${sub.name}</option>`;
            });
            subCatDropdown.innerHTML = options;
        })
        .catch(error => {
            console.error("Error loading subcategories:", error);
            subCatDropdown.innerHTML = '<option value="">Error loading</option>';
        });
}