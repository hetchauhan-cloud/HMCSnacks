$(document).ready(function () {
    $('#StateId').change(function () {
        var stateId = $(this).val();
        $('#CityId').html('<option value="">Loading...</option>');

        if (stateId) {
            $.ajax({
                url: '/api/location/GetCitiesByStateId/' + stateId,
                method: 'GET',
                success: function (data) {
                    var cityDropdown = $('#CityId');
                    cityDropdown.empty();
                    cityDropdown.append('<option value="">-- Select City --</option>');
                    $.each(data, function (i, city) {
                        cityDropdown.append('<option value="' + city.id + '">' + city.cityName + '</option>');
                    });
                },
                error: function (xhr) {
                    console.error("Failed to load cities", xhr);
                    alert("Could not load cities.");
                }
            });
        } else {
            $('#CityId').html('<option value="">-- Select City --</option>');
        }
    });
});

const maxAddresses = 4;
let addressIndex = 1;

const labelOptions = ["Home", "Work", "Other1", "Other2"];

function addAddress() {
    const container = document.getElementById("addressContainer");
    const usedLabels = Array.from(container.querySelectorAll(".address-label"))
        .map(s => s.value.toLowerCase());

    if (usedLabels.length >= maxAddresses) {
        alert("Maximum 4 addresses allowed.");
        return;
    }

    const availableLabels = labelOptions.filter(l => !usedLabels.includes(l.toLowerCase()));
    if (availableLabels.length === 0) return;

    const label = availableLabels[0];

    const block = document.createElement("div");
    block.className = "address-block mb-2";

    block.innerHTML = `
        <div class="row g-2">
            <div class="col-md-4">
                <select class="form-select address-label" name="Addresses[${addressIndex}].Label" onchange="updateInputName(this)">
                    <option value="">-- Select Label --</option>
                    ${labelOptions.map(l =>
        `<option value="${l}" ${l === label ? 'selected' : ''}>${l.replace('Other1', 'Other 1').replace('Other2', 'Other 2')}</option>`).join('')}
                </select>
            </div>
            <div class="col-md-8">
                <input type="text" class="form-control address-input" name="Addresses[${addressIndex}].AddressLine1" placeholder="Enter Address" />
            </div>
        </div>
    `;

    container.appendChild(block);
    addressIndex++;
}

function updateInputName(selectElement) {
    const row = selectElement.closest(".row");
    const input = row.querySelector(".address-input");
    const block = selectElement.closest(".address-block");
    const allBlocks = Array.from(document.querySelectorAll("#addressContainer .address-block"));

    const newIndex = allBlocks.indexOf(block);
    if (newIndex !== -1) {
        selectElement.name = `Addresses[${newIndex}].Label`;
        input.name = `Addresses[${newIndex}].AddressLine1`;
    }
}


