//Function is only used for posting data. Getting data is done on the server
const SendD = (JSONdata) => {
        var xmlhttp = new XMLHttpRequest();
        xmlhttp.onreadystatechange = function()
        {
            if(this.readyState === 4 && this.status === 200)
            {
                var JSON_= JSON.parse(this.responseText);
				c_ajax.final_ajax(JSON_.FORM, JSON_.DATA);
				return;
            }
        };

        try
        {
            xmlhttp.open("POST", project , true);
            xmlhttp.setRequestHeader('Content-type', 'application/x-www-form-urlencoded');
            xmlhttp.send(JSONdata);
        }
        catch(e)
        {
            console.log("An error occured [" + e + "]");
        }
        return;
}