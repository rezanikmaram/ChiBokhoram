function ShowQAlert(event, id, title, message) {
    event.preventDefault();

    let t = title || 'تایید عملیات';
    let msg = message || 'آیا عملیات را تایید می کنید؟';

    swal.fire({
        title: t,
        text: msg,
        icon: "question",
        showCancelButton: true,
        confirmButtonText: "بله",
        cancelButtonText: "لغو",
    }).then((result) => {
        if (result.isConfirmed) {
            var url = document.getElementById(id).getAttribute("href");
            window.location = url;
        }

    });
}


async function ShowQAlert2(event, title, message) {
    event.preventDefault();

    let t = title || 'تایید عملیات';
    let msg = message || 'آیا عملیات را تایید می کنید؟';

    let result = await swal.fire({
        title: t,
        text: msg,
        icon: "question",
        showCancelButton: true,
        confirmButtonText: "بله",
        cancelButtonText: "لغو",
    });

    return result.isConfirmed;
}

function sendNotification(controller, id) {

    let t = 'ارسال اعلان';
    let msg =  'دستور ارسال اعلان را تایید می کنید؟';

    swal.fire({
        title: t,
        text: msg,
        icon: "question",
        showCancelButton: true,
        confirmButtonText: "بله",
        cancelButtonText: "لغو",
    }).then((result) => {
        if (result.isConfirmed) {
            $.post("/" + controller + "/SendNotif", { id: $("#g_" + id).val() }).done(function (res) {
                if (res.mode == "ok") {
                    Toast.fire({
                        icon: "success",
                        title: res.msg
                    });
                }
                else {
                    Toast.fire({
                        icon: "error",
                        title: res.msg
                    });
                }

            });
        }

    });
 
}




function deletepersonfromgroup(id, personGroup) {
    let t = 'حذف یک عضو';
    let msg = 'از حذف این شخص از گروه مطمئن هستید؟';

    swal.fire({
        title: t,
        text: msg,
        icon: "question",
        showCancelButton: true,
        confirmButtonText: "بله",
        cancelButtonText: "لغو",
    }).then((result) => {
        if (result.isConfirmed) {
            $.post("/Personel/DeletePersonFromGroup", { personGroupId: id, personGroup: personGroup }).done(function (res) {

                if (res.mode == "ok") {
                    Toast.fire({
                        icon: "success",
                        title: res.msg
                    });
                    $("#listuser").html(res.source);
                    $("#usercount").html(res.usercount);
                }
                else {
                    Toast.fire({
                        icon: "error",
                        title: res.msg
                    });
                }

            });
             
        }

    });

}
