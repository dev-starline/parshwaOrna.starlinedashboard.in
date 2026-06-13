$(document).ready(function () {
    $("#myInput").on("keyup", function () {
        var value = $(this).val().toLowerCase();
        $("#myTable tbody tr").filter(function () {
            $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)
        });
    });
});

function validateImageSize(input, errorSpanId, fileNameSpanId) {
    const file = input.files[0];
    if (file) {
        const fileName = file.name;
        const img = new Image();
        const objectUrl = URL.createObjectURL(file);

        img.onload = function () {
            if (this.width !== 700 || this.height !== 480) {
                document.getElementById(errorSpanId).style.display = "block";
                input.value = ""; // clear invalid file
                document.getElementById(fileNameSpanId).textContent = "No file chosen"; // clear name
            } else {
                document.getElementById(errorSpanId).style.display = "none";
                document.getElementById(fileNameSpanId).textContent = fileName; //  show valid file name
            }
            URL.revokeObjectURL(objectUrl);
        };
        img.src = objectUrl;
    }
}

//function validateAndCompressImage(input, fileNameSpanId) {
//    const file = input.files[0];
//    const fileNameSpan = document.getElementById(fileNameSpanId);

//    if (!fileNameSpan) {
//        console.error("Span not found:", fileNameSpanId);
//        return;
//    }

//    if (!file) {
//        fileNameSpan.textContent = "No file chosen";
//        return;
//    }

//    const fileName = file.name.length > 30 ? file.name.substring(0, 30) + "..." : file.name;

//    fileNameSpan.textContent = fileName;
//    fileNameSpan.title = file.name;

//    const reader = new FileReader();
//    reader.onload = function (event) {
//        const img = new Image();
//        img.src = event.target.result;

//        img.onload = function () {
//            const canvas = document.createElement("canvas");
//            const ctx = canvas.getContext("2d");

//            let width = img.width;
//            let height = img.height;

//            canvas.width = width;
//            canvas.height = height;
//            ctx.drawImage(img, 0, 0, width, height);

//            canvas.toBlob(
//                function (blob) {
//                    const compressedFile = new File([blob], file.name, {
//                        type: "image/jpeg",
//                        lastModified: Date.now(),
//                    });

//                    console.log("Compressed file size:", (compressedFile.size / 1024).toFixed(2), "KB");
//                },
//                "image/jpeg",
//                0.7
//            );
//        };
//    };

//    reader.readAsDataURL(file);
//}

function validateAndCompressImage(input, fileNameSpanId) {
    const file = input.files[0];
    const fileNameSpan = document.getElementById(fileNameSpanId);

    if (!fileNameSpan) {
        console.error("Span not found:", fileNameSpanId);
        return;
    }

    if (!file) {
        fileNameSpan.textContent = "No file chosen";
        return;
    }

    const fileName = file.name.length > 30
        ? file.name.substring(0, 30) + "..."
        : file.name;

    fileNameSpan.textContent = fileName;
    fileNameSpan.title = file.name;

    const reader = new FileReader();
    reader.onload = function (event) {
        const img = new Image();
        img.src = event.target.result;

        img.onload = function () {
            const canvas = document.createElement("canvas");
            const ctx = canvas.getContext("2d");

            canvas.width = img.width;
            canvas.height = img.height;

            ctx.drawImage(img, 0, 0);

            canvas.toBlob(function (blob) {
                const compressedFile = new File([blob], file.name, {
                    type: "image/jpeg",
                    lastModified: Date.now(),
                });

                console.log("Compressed file size:",
                    (compressedFile.size / 1024).toFixed(2), "KB");
            }, "image/jpeg", 0.7);
        };
    };

    reader.readAsDataURL(file);
}

function validateCategoryDropdown() {
    const category = document.getElementById("CategoryId").value;
    const subcategory = document.getElementById("SubCategoryId").value;

    if (!category) {
        alert("Please select category");
        return false; // stop form submission
    }
    if (!subcategory) {
        alert("Please select sub-category");
        return false;
    }

    return true; // allow form submission
}

$(function () {
    $("#datatable").DataTable({
        "paging": false,
        lengthChange: false,
        "searching": true,
        "ordering": true,
        "info": false,
        "autoWidth": false,
        "responsive": true,
        responsive: true
    });
});

