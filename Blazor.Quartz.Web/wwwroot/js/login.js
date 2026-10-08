//登录/退出登录辅助：通过浏览器fetch发起同源POST请求，
//使Set-Cookie响应头经由浏览器生效（Blazor服务端电路内无法直接使用HttpContext签发Cookie）
window.loginManager = {
    postJson: async function (url, json) {
        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: json,
            credentials: 'same-origin'
        });
        return await response.text();
    }
};
