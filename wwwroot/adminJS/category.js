function validateCategoryName(form) {
    const nameInput = form.querySelector('#categoryName');
    const errorMsg = document.getElementById('nameErrorMsg');

    if (!nameInput.value.trim()) {
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

function validateCategoryDropdown() {
    const category = document.getElementById("CategoryId").value;
    if (!category) {
        alert("Please select category");
        return false; 
    }
    return true; 
}

function loadFirstSubCategory(categoryId) {
	const subCatDropdown = document.getElementById("SubCategoryId");
	subCatDropdown.innerHTML = '<option value="">Loading...</option>';

	if (!categoryId) {
		subCatDropdown.innerHTML = '<option value="">Select Sub-Category</option>';
		return;
	}

	fetch(`/Jewellery/GetFirstSubCategory?categoryId=${categoryId}`)
		.then(response => response.json())
		.then(data => {
			if (data && data.length > 0) {
				let options = "";

				data.forEach((subCat, index) => {
					const selected = index === 0 ? "selected" : "";
					options += `<option value="${subCat.id}" ${selected}>${subCat.name}</option>`;
				});

				subCatDropdown.innerHTML = options;
			} else {
				subCatDropdown.innerHTML = '<option value="">No SubCategory Found</option>';
			}
		})
		.catch(error => {
			console.error("Error:", error);
			subCatDropdown.innerHTML = '<option value="">Error loading</option>';
		});
}