export async function handleFetch({ event, request, fetch }) {
    const url = new URL(request.url);

    console.log(`Request URL: ${url.toString()}`)
    return await fetch(request);
}