const HEADERS = new Headers({
    'Content-Type': 'application/json;charset=UTF-8',
    "User-Agent": "FFXIV Gacha"
});

export async function getLastUpdate(): Promise<string> {
    let data = await fetch('data/LastUpdate.json', {
        method: 'GET',
        headers: HEADERS
    })
    .then(responseHandler)
    .then((data: string) => {
        return data;
    })
    .catch(errorHandling);

    return data ?? 'Unknown';
}

function responseHandler(response: Response) {
    if (response.ok) {
        return response.json();
    }

    return Promise.reject(response);
}

function errorHandling(response: any) {
    if (response instanceof Response) {
        console.log(response.status, response.statusText);
        response.json().then((err: any) => {
            console.log(err);
        })

        return;
    }

    console.log(response);
    return;

}