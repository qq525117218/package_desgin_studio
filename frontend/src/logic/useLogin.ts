// src/logic/useLogin.ts
import { reactive, ref } from 'vue'
import { ElMessage, type FormInstance, type FormRules } from 'element-plus'

// 1. 根据你提供的 JSON 定义接口
export interface LoginResponse {
    code: number
    is_success: boolean
    message: string
    request_id: string
    data: {
        token: string
        expire_at: string
    }
}

export function useLogin(emit: (event: 'login-success', username: string) => void) {
    const formRef = ref<FormInstance>()
    const isLoading = ref(false)

    const loginForm = reactive({
        username: '',
        password: ''
    })

    const rules = reactive<FormRules>({
        username: [{ required: true, message: '请输入账号', trigger: 'blur' }],
        password: [{ required: true, message: '请输入密码', trigger: 'blur' }]
    })

    const handleLogin = async () => {
        if (!formRef.value) return

        await formRef.value.validate(async (valid) => {
            if (valid) {
                isLoading.value = true
                try {
                    // 2. 使用原生 fetch，避免被 src/utils/request.ts 的拦截器误伤
                    const response = await fetch('/api/auth/login', {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json'
                        },
                        body: JSON.stringify({
                            username: loginForm.username,
                            password: loginForm.password
                        })
                    })

                    // 3. 单独处理 401，这里代表账号密码错误，而不是 Token 过期
                    if (response.status === 401) {
                        ElMessage.error('账号或密码错误，请检查')
                        return
                    }

                    if (!response.ok) {
                        ElMessage.error(`服务响应异常: ${response.status}`)
                        return
                    }

                    const resData = (await response.json()) as LoginResponse

                    // 4. 处理业务逻辑
                    if (resData.code === 200 && resData.is_success) {
                        ElMessage.success(resData.message || '登录成功')

                        // 存储 Token 和过期时间
                        localStorage.setItem('token', resData.data.token)
                        localStorage.setItem('username', loginForm.username)
                        localStorage.setItem('token_expire', resData.data.expire_at)

                        emit('login-success', loginForm.username)
                    } else {
                        ElMessage.error(resData.message || '登录失败')
                    }
                } catch (error: any) {
                    console.error('Login error:', error)
                    ElMessage.error('网络连接失败')
                } finally {
                    isLoading.value = false
                }
            }
        })
    }

    return { formRef, isLoading, loginForm, rules, handleLogin }
}