// src/utils/request.ts

/**
 * 带有自动重试机制的 Fetch 封装
 * 解决服务器时钟偏移导致的偶发 401 问题
 */
export async function authFetch(url: string, options: RequestInit = {}, retryCount = 0) {
    // 1. 获取认证信息
    const token = localStorage.getItem('token')
    const expireStr = localStorage.getItem('token_expire')

    // 2.【前置防御】本地过期检查
    // 如果本地时间已经超过了 token_expire，直接判定失效，不发送请求给后端
    if (token && expireStr) {
        const expireTime = new Date(expireStr).getTime()
        const now = new Date().getTime()

        // 预留 5 秒的缓冲期
        if (now > expireTime - 5000) {
            console.warn('Token expired locally. Request blocked.')
            handleUnauthorized()
            throw new Error('Local Token Expired')
        }
    }

    // 3. 构造请求头
    const headers = new Headers(options.headers)
    if (token) {
        headers.set('Authorization', `Bearer ${token}`)
    }
    if (!headers.has('Content-Type') && !(options.body instanceof FormData)) {
        headers.set('Content-Type', 'application/json')
    }

    const config = { ...options, headers }

    try {
        const response = await fetch(url, config)

        // 4.【核心修复】401 智能重试逻辑
        if (response.status === 401) {
            // 如果是第一次遇到 401，且我们认为可能是时钟偏差 (Clock Skew) 导致的
            // 我们尝试等待 500ms 后重试一次
            if (retryCount < 1) {
                console.warn(`Encountered 401. Retrying... (${retryCount + 1}/1)`)

                // 延迟 500ms，让 Token "生效" (解决 nbf 问题)
                await new Promise(resolve => setTimeout(resolve, 500))

                // 递归调用自己，retryCount + 1
                return authFetch(url, options, retryCount + 1)
            } else {
                // 如果重试后依然是 401，说明是真的过期了
                console.warn('Retry failed. Unauthorized.')
                handleUnauthorized()
                throw new Error('Unauthorized')
            }
        }

        return response
    } catch (error) {
        throw error
    }
}

/**
 * 统一处理未授权逻辑
 */
function handleUnauthorized() {
    // 避免短时间内重复触发
    // 使用 CustomEvent 通知 App.vue 执行登出
    window.dispatchEvent(new CustomEvent('auth:unauthorized'))
}