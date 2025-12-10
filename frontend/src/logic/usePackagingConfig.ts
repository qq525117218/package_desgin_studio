import { reactive, ref } from 'vue'
import { ElMessage, ElLoading, type FormInstance, type FormRules, type UploadFile } from 'element-plus'

// --- 全局配置 ---
const OSS_BASE_URL = 'https://oss-pro.plm.westmonth.cn'

// --- 接口定义 ---

// 1. 基础数据结构
export interface Dimensions { length: number; width: number; height: number; bleedX: number; bleedY: number; bleedInner: number; }

// Content 接口：对应 Step 3 文档解析的数据 (通常是实际生产工厂信息)
export interface Content {
    productName: string;
    ingredients: string;
    warnings: string;
    manufacturer: string; // 实际生产商 (Factory)
    origin: string;
    shelfLife: string;
    address: string;      // 实际生产地址 (Factory Address)
    directions: string;
    benefits: string;     // [新增] 产品功效
}

// Marketing 接口：对应 Step 2 产品定义的数据 (通常是品牌方/经销商信息)
export interface Marketing {
    sku: string;
    brand: string;
    capacityValue: string;     // 正面规格
    capacityValueBack: string; // 背面规格
    capacityUnit: string;
    sellingPoints: string[];
    // [New] 新增独立字段，与 Content 解耦
    manufacturer: string; // 品牌商/经销商 (Distributor/Brand Owner)
    address: string;      // 品牌商地址 (Distributor Address)
}

export interface WorkflowData {
    dimensions: Dimensions;
    content: Content;
    marketing: Marketing;
}

// 2. 文档解析响应接口
interface ParseDocResponse {
    code: number
    is_success: boolean
    message: string
    data: {
        content: {
            product_name: string
            manufacturer: string
            country_of_origin: string
            warnings: string
            shelf_life: string
            address: string
            directions: string
            benefits: string // [新增]
            ingredients: {
                active_ingredients: string
                inactive_ingredients: string
                raw_text: string
            }
        }
    }
}

// 3. 品牌相关接口
export interface BrandItem {
    id: number
    code: string
    name: string
    abbr: string
    brand_category_name: string
    department_name: string
    status: number
    is_deleted: number
}

interface BrandListResponse {
    code: number
    is_success: boolean
    message: string
    request_id: string
    data: {
        plm_brand_data: BrandItem[]
    }
}

interface BrandDetailResponse {
    code: number
    is_success: boolean
    message: string
    data: {
        name: string
        default_manufacturer?: {
            manufacturer_english_name: string
            manufacturer_english_address: string
        }
    }
}

// 4. 条码相关接口
interface BarcodeResponse {
    code: number
    is_success: boolean
    message: string
    request_id: string
    data: {
        bar_code: string
        bar_code_path: string
    }
}

// 5. 任务与进度接口
interface PsdTaskStatus {
    task_id: string
    progress: number
    status: 'Pending' | 'Processing' | 'Completed' | 'Failed'
    message: string
    download_url?: string
}

interface AsyncSubmitResponse {
    code: number
    is_success: boolean
    message: string
    data: string // taskId
}

interface ProgressResponse {
    code: number
    is_success: boolean
    data: PsdTaskStatus
}

// --- 主要逻辑 Hook ---
// [修改] 接收 onUnauthorized 回调参数
export function usePackagingConfig(onUnauthorized: () => void) {
    const activeStep = ref(0)
    const formRef = ref<FormInstance>()
    const isDocParsed = ref(false)
    const fileName = ref('')
    const inputValue = ref('')

    // --- 状态管理 ---
    const isGenerating = ref(false)
    const progressPercentage = ref(0)
    const progressStatus = ref('')
    const progressMessage = ref('准备提交任务...')
    const currentDownloadUrl = ref('')
    const currentTaskId = ref('')
    const generatedFileName = ref('')
    const brandOptions = ref<BrandItem[]>([])
    const isFetchingBarcode = ref(false)
    const barcodeUrl = ref('')

    // 初始化数据
    const getInitialData = (): WorkflowData => ({
        dimensions: { length: 6, width: 6, height: 12, bleedX: 0.5, bleedY: 2, bleedInner: 0.15 },
        content: {
            productName: '', ingredients: '', warnings: '', manufacturer: '',
            origin: '', shelfLife: '', address: '', directions: '',
            benefits: '' // [新增] 初始化
        },
        marketing: {
            sku: '',
            brand: 'WestMoon',
            capacityValue: 'NET：100G/3.53 FL.OZ',
            capacityValueBack: 'NET：100G/3.53 FL.OZ',
            capacityUnit: '',
            sellingPoints: [
                'Professional-grade anti-fog solution.',
                'Effective long-lasting anti-fog.',
                'Versatile for multiple surfaces.',
                'Safe reef-friendly ingredients.'
            ],
            // [New] 初始化新增字段
            manufacturer: '',
            address: ''
        }
    })

    const formData = reactive<WorkflowData>(getInitialData())

    // 验证规则
    const rules = reactive<FormRules>({
        'dimensions.length': [{ required: true, message: 'Required', trigger: 'blur' }],
        'dimensions.width': [{ required: true, message: 'Required', trigger: 'blur' }],
        'dimensions.height': [{ required: true, message: 'Required', trigger: 'blur' }],
        'content.productName': [{ required: true, message: '请上传文档', trigger: 'change' }],
        'marketing.sku': [{ required: true, message: '请输入 SKU', trigger: 'blur' }],
        'marketing.brand': [{ required: true, message: '请选择品牌', trigger: 'change' }],
        'marketing.capacityValue': [{ required: true, message: '请输入正面规格', trigger: 'blur' }],
        'marketing.capacityValueBack': [{ required: true, message: '请输入背面规格', trigger: 'blur' }]
    })

    // [新增] 鉴权 Fetch 封装
    const authFetch = async (url: string, options: RequestInit = {}) => {
        const token = localStorage.getItem('token')

        // 1. 本地无 Token，直接退出
        if (!token) {
            console.warn('No Access Token found locally.')
            onUnauthorized()
            throw new Error('No Access Token')
        }

        const headers = {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`,
            ...(options.headers || {})
        }

        try {
            const response = await fetch(url, { ...options, headers })

            // 2. 拦截 401 Unauthorized
            if (response.status === 401) {
                console.warn('Received 401 Unauthorized from server.')
                onUnauthorized() // 触发登出
                throw new Error('Unauthorized') // 中断后续代码
            }

            return response
        } catch (error) {
            throw error
        }
    }

    // 步骤验证配置
    const stepValidationConfig: Record<number, string[]> = {
        0: ['dimensions.length', 'dimensions.width', 'dimensions.height'],
        1: ['marketing.sku', 'marketing.brand', 'marketing.capacityValue', 'marketing.capacityValueBack'],
        2: ['content.productName']
    }

    // --- 流程控制 ---
    const nextStep = async () => {
        if (!formRef.value) return;
        if (activeStep.value === 2 && !isDocParsed.value) { ElMessage.warning('请上传文档'); return; }
        const fieldsToValidate = stepValidationConfig[activeStep.value] || []
        if (fieldsToValidate.length > 0) {
            await formRef.value.validateField(fieldsToValidate, (isValid) => { if (isValid) activeStep.value++ })
        } else { activeStep.value++ }
    }

    const prevStep = () => { if (activeStep.value > 0) activeStep.value-- }

    const resetWorkflow = () => {
        Object.assign(formData, getInitialData())
        isDocParsed.value = false
        fileName.value = ''
        generatedFileName.value = ''
        activeStep.value = 0
        currentDownloadUrl.value = ''
        currentTaskId.value = ''
        barcodeUrl.value = ''
    }

    // --- 文件处理 ---
    const fileToBase64 = (file: File): Promise<string> => {
        return new Promise((resolve, reject) => {
            const reader = new FileReader()
            reader.readAsDataURL(file)
            reader.onload = () => {
                const result = reader.result as string
                const base64Content = result.split(',')[1]
                if (base64Content) resolve(base64Content)
                else reject(new Error('Failed to parse base64 content'))
            }
            reader.onerror = (error) => reject(error)
        })
    }

    const handleFileUpload = async (file: UploadFile) => {
        if (!file.raw) return
        const loadingInstance = ElLoading.service({ text: 'AI 解析中...', background: 'rgba(255,255,255,0.8)' })
        try {
            const base64String = await fileToBase64(file.raw)
            // [修改] 使用 authFetch
            const response = await authFetch('/api/document/parse/word', {
                method: 'POST',
                body: JSON.stringify({ file_name: file.name, file_content_base64: base64String })
            })
            const resData = (await response.json()) as ParseDocResponse

            if (response.ok && resData.code === 200 && resData.is_success && resData.data) {
                const parsed = resData.data.content
                Object.assign(formData.content, {
                    productName: parsed.product_name || '',
                    manufacturer: parsed.manufacturer || '',
                    origin: parsed.country_of_origin || '',
                    warnings: parsed.warnings || '',
                    shelfLife: parsed.shelf_life || '',
                    address: parsed.address || '',
                    directions: parsed.directions || '',
                    benefits: parsed.benefits || '', // [新增] 映射解析结果
                    ingredients: parsed.ingredients?.raw_text || (parsed.ingredients?.active_ingredients ? `Active: ${parsed.ingredients.active_ingredients}\n` : '') + (parsed.ingredients?.inactive_ingredients ? `Inactive: ${parsed.ingredients.inactive_ingredients}` : '')
                })
                fileName.value = file.name
                isDocParsed.value = true
                ElMessage.success('解析成功')
            } else { throw new Error('解析失败') }
        } catch (error: any) {
            // [修改] 忽略 Unauthorized 错误，避免重复提示
            if (error.message !== 'Unauthorized') {
                ElMessage.error(error.message || '解析异常')
            }
            isDocParsed.value = false
        } finally { loadingInstance.close() }
    }

    // --- 标签处理 ---
    const handleCloseTag = (tag: string) => {
        formData.marketing.sellingPoints.splice(formData.marketing.sellingPoints.indexOf(tag), 1)
    }
    const handleInputConfirm = () => {
        if (inputValue.value) {
            formData.marketing.sellingPoints.push(inputValue.value); inputValue.value = ''
        }
    }
    const addQuickTag = (tag: string) => {
        if (!formData.marketing.sellingPoints.includes(tag)) formData.marketing.sellingPoints.push(tag)
    }

    // --- 品牌切换处理 ---
    const handleBrandChange = async (brandName: string) => {
        if (!brandName) return
        const brand = brandOptions.value.find(b => b.name === brandName)
        if (!brand || !brand.code) return

        const loading = ElLoading.service({ text: '正在获取品牌制造商信息...', background: 'rgba(255,255,255,0.6)' })
        try {
            // [修改] 使用 authFetch
            const response = await authFetch('/api/plm/brand/detail', {
                method: 'POST',
                body: JSON.stringify({ code: brand.code })
            })
            const resData = await response.json() as BrandDetailResponse

            if (resData.is_success && resData.data?.default_manufacturer) {
                formData.marketing.manufacturer = resData.data.default_manufacturer.manufacturer_english_name || ''
                formData.marketing.address = resData.data.default_manufacturer.manufacturer_english_address || ''
                ElMessage.success('已更新品牌方信息')
            }
        } catch (error) {
            console.error('Fetch brand detail failed', error)
            // 静默失败或提示
        } finally {
            loading.close()
        }
    }

    // --- 条码获取 ---
    const handleFetchBarcode = async () => {
        const skuCode = formData.marketing.sku
        if (!skuCode) return

        barcodeUrl.value = ''
        isFetchingBarcode.value = true

        try {
            // [修改] 使用 authFetch
            const response = await authFetch('/api/plm/product/barcode', {
                method: 'POST',
                body: JSON.stringify({ code: skuCode })
            })

            const resData = (await response.json()) as BarcodeResponse

            if (response.ok && resData.is_success && resData.data?.bar_code_path) {
                const path = resData.data.bar_code_path
                const fullUrl = path.startsWith('http') ? path : `${OSS_BASE_URL}${path}`
                barcodeUrl.value = fullUrl
            } else {
                console.warn('Barcode not found or API error', resData.message)
            }

        } catch (error) {
            console.error('Failed to fetch barcode', error)
        } finally {
            isFetchingBarcode.value = false
        }
    }

    // --- PSD 生成 ---
    const handleGeneratePSD = async () => {
        isGenerating.value = true
        progressPercentage.value = 0
        progressStatus.value = ''
        progressMessage.value = '正在提交生成任务...'

        try {
            const username = localStorage.getItem('username') || 'User'

            // --- 构建 Payload ---
            const payload = {
                project_name: `${formData.marketing.brand}_${formData.marketing.sku}`.replace(/\s+/g, '_'),
                user_context: { username: username, generate_dieline: true },
                specifications: {
                    dimensions: formData.dimensions,
                    print_config: { bleed_x: formData.dimensions.bleedX, bleed_y: formData.dimensions.bleedY, bleed_inner: formData.dimensions.bleedInner, resolution_dpi: 300 }
                },
                assets: {
                    texts: {
                        main_panel: {
                            brand_name: formData.marketing.brand,
                            capacity_info: formData.marketing.capacityValue,
                            capacity_info_back: formData.marketing.capacityValueBack,
                            selling_points: formData.marketing.sellingPoints,
                            manufacturer: formData.marketing.manufacturer,
                            address: formData.marketing.address
                        },
                        info_panel: {
                            product_name: formData.content.productName,
                            shelf_life: formData.content.shelfLife,
                            ingredients: formData.content.ingredients,
                            manufacturer: formData.content.manufacturer,
                            origin: formData.content.origin,
                            warnings: formData.content.warnings,
                            directions: formData.content.directions,
                            address: formData.content.address,
                            benefits: formData.content.benefits
                        }
                    },
                    dynamic_images: {
                        barcode: {
                            value: formData.marketing.sku,
                            type: 'EAN-13',
                            url: barcodeUrl.value
                        }
                    }
                }
            }

            // [修改] 使用 authFetch
            const submitRes = await authFetch('/api/design/generate/psd/async', {
                method: 'POST',
                body: JSON.stringify(payload)
            })

            if (!submitRes.ok) throw new Error(`提交失败: ${submitRes.status}`)
            const submitData = (await submitRes.json()) as AsyncSubmitResponse
            if (!submitData.is_success) throw new Error(submitData.message)

            const taskId = submitData.data
            currentTaskId.value = taskId

            await pollProgress(taskId, payload.project_name)

        } catch (error: any) {
            // [修改] 忽略 Unauthorized
            if (error.message !== 'Unauthorized') {
                console.error('Task Failed:', error)
                progressStatus.value = 'exception'
                progressMessage.value = `错误: ${error.message || '未知错误'}`
                ElMessage.error('生成失败')
                setTimeout(() => { isGenerating.value = false }, 3000)
            }
        }
    }

    const pollProgress = async (taskId: string, defaultName: string) => {
        return new Promise<void>((resolve, reject) => {
            const timer = setInterval(async () => {
                try {
                    // [修改] 使用 authFetch
                    const QX = await authFetch(`/api/design/progress/${taskId}`)

                    if (!QX.ok) throw new Error('无法获取任务进度')
                    const resData = (await QX.json()) as ProgressResponse
                    const task = resData.data

                    progressPercentage.value = task.progress
                    progressMessage.value = task.message || '正在处理...'

                    if (task.status === 'Completed') {
                        clearInterval(timer)
                        progressStatus.value = 'success'
                        progressMessage.value = '生成完成，即将下载...'

                        const downloadUrl = task.download_url
                            ? task.download_url
                            : `/api/design/download/${taskId}?fileName=${defaultName}.psd`

                        currentDownloadUrl.value = downloadUrl

                        try {
                            const urlObj = new URL(downloadUrl, window.location.origin)
                            const finalFileName = urlObj.searchParams.get('fileName')
                            generatedFileName.value = finalFileName ? decodeURIComponent(finalFileName) : `${defaultName}.psd`
                        } catch (e) {
                            generatedFileName.value = `${defaultName}.psd`
                        }

                        await triggerDownload(downloadUrl)

                        setTimeout(() => {
                            isGenerating.value = false
                            activeStep.value = 4
                            resolve()
                        }, 1000)
                    }
                    else if (task.status === 'Failed') {
                        clearInterval(timer)
                        throw new Error(task.message || '生成失败')
                    }
                } catch (err) {
                    clearInterval(timer)
                    reject(err)
                }
            }, 1000)
        })
    }

    const triggerDownload = (url: string) => {
        const link = document.createElement('a')
        link.href = url
        link.setAttribute('download', '')
        document.body.appendChild(link)
        link.click()
        document.body.removeChild(link)
        ElMessage.success('下载已开始，请留意浏览器下载任务')
    }

    const fetchBrandList = async () => {
        try {
            // [修改] 使用 authFetch
            const response = await authFetch('/api/plm/brand/list')
            const resData = await response.json() as BrandListResponse
            if (resData.is_success) brandOptions.value = resData.data.plm_brand_data
        } catch (e) { console.error(e) }
    }
    fetchBrandList()

    return {
        activeStep, formRef, formData, rules, isDocParsed, fileName, inputValue, brandOptions,
        isGenerating, progressPercentage, progressStatus, progressMessage, currentDownloadUrl, generatedFileName,
        isFetchingBarcode, barcodeUrl,
        nextStep, prevStep, resetWorkflow, handleFileUpload, handleCloseTag, handleInputConfirm, addQuickTag, handleGeneratePSD, triggerDownload,
        handleBrandChange,
        handleFetchBarcode
    }
}