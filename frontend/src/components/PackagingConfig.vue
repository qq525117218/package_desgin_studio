<template>
  <div class="workflow-page">
    <header class="app-header">
      <div class="header-inner">
        <div class="brand">
          <div class="logo-box">📦</div>
          <span class="app-title">Package Desgin Studio <span class="version">PRO</span></span>
        </div>
        <div class="user-profile">
          <span class="welcome-text">Hi, {{ username }}</span>
          <el-dropdown trigger="click">
            <div class="avatar-wrapper">
              <el-avatar :size="32" class="user-avatar">
                {{ username ? username.charAt(0).toUpperCase() : 'U' }}
              </el-avatar>
            </div>
            <template #dropdown>
              <el-dropdown-menu>
                <el-dropdown-item disabled>个人中心</el-dropdown-item>
                <el-dropdown-item divided @click="$emit('logout')">退出登录</el-dropdown-item>
              </el-dropdown-menu>
            </template>
          </el-dropdown>
        </div>
      </div>
    </header>

    <div class="workflow-body">
      <div class="steps-container">
        <div class="glass-steps-panel">
          <el-steps :active="activeStep" finish-status="success" align-center class="premium-steps">
            <el-step title="规格定义" description="Dimensions" icon="Box" />
            <el-step title="产品定义" description="Identity" icon="PriceTag" />
            <el-step title="文案解析" description="Analysis" icon="DocumentChecked" />
            <el-step title="AI设计" description="AI Creative" icon="MagicStick" />
            <el-step title="构建交付" description="Delivery" icon="Files" />
          </el-steps>
        </div>
      </div>
      <div class="workspace-container">
        <el-form
            v-if="activeStep < 5"
            ref="formRef"
            :model="formData"
            :rules="rules"
            label-position="top"
            class="workspace-form"
            size="large"
            hide-required-asterisk
        >
          <transition name="slide-fade" mode="out-in">
            <div v-if="activeStep === 0" key="step1" class="step-panel step-dimensions">
              <div class="panel-header">
                <h2>定义包装规格</h2>
                <p>设定包装盒的物理切割尺寸与印刷工艺参数。</p>
              </div>

              <div class="dimensions-stage">
                <div class="physical-zone">
                  <div class="box-visual">
                    <div class="grid-bg"></div>
                    <div class="cube-wrapper" :style="cubeStyle">
                      <div class="cube">
                        <div class="face front"><div class="face-label"><span class="en">FRONT</span><span class="cn">正面</span></div></div>
                        <div class="face back"><div class="face-label"><span class="en">BACK</span><span class="cn">背面</span></div></div>
                        <div class="face right"><div class="face-label"><span class="en">RIGHT</span><span class="cn">右侧面</span></div></div>
                        <div class="face left"><div class="face-label"><span class="en">LEFT</span><span class="cn">左侧面</span></div></div>
                        <div class="face top"><div class="face-label"><span class="en">TOP</span><span class="cn">顶面</span></div></div>
                        <div class="face bottom"><div class="face-label"><span class="en">BOTTOM</span><span class="cn">底面</span></div></div>
                        <div class="inner-core"></div>
                      </div>
                      <div class="shadow-dynamic"></div>
                    </div>
                  </div>
                  <div class="main-inputs">
                    <div class="input-card l-axis">
                      <div class="label-row"><el-icon><DArrowRight /></el-icon> <span class="cn">长度</span>Length</div>
                      <div class="input-wrapper"><el-input-number v-model="formData.dimensions.length" :min="0" :controls="false" class="big-num-input" placeholder="0.0" /><span class="unit-tag">cm</span></div>
                    </div>
                    <div class="input-card w-axis">
                      <div class="label-row"><el-icon><DArrowLeft /></el-icon> <span class="cn">宽度</span> Width</div>
                      <div class="input-wrapper"><el-input-number v-model="formData.dimensions.width" :min="0" :controls="false" class="big-num-input" placeholder="0.0" /><span class="unit-tag">cm</span></div>
                    </div>
                    <div class="input-card h-axis">
                      <div class="label-row"><el-icon><Top /></el-icon><span class="cn">高度</span>  Height</div>
                      <div class="input-wrapper"><el-input-number v-model="formData.dimensions.height" :min="0" :controls="false" class="big-num-input" placeholder="0.0" /><span class="unit-tag">cm</span></div>
                    </div>
                  </div>
                </div>

                <div class="tech-dock-panel">
                  <div class="dock-title-block">
                    <div class="icon-skin"><el-icon><Scissor /></el-icon></div>
                    <div class="text-group"><span class="cn">包装工艺参数</span><span class="en">Process Specs</span></div>
                  </div>
                  <div class="modules-container">
                    <div class="spec-module-card"><div class="card-label"><span class="cn">左右出血</span><span class="en">Bleed X</span></div><div class="card-input-row"><el-input-number v-model="formData.dimensions.bleedX" :step="0.5" :min="0" :controls="false" class="module-input" /><span class="unit">cm</span></div></div>
                    <div class="spec-module-card"><div class="card-label"><span class="cn">上下出血</span><span class="en">Bleed Y</span></div><div class="card-input-row"><el-input-number v-model="formData.dimensions.bleedY" :step="0.5" :min="0" :controls="false" class="module-input" /><span class="unit">cm</span></div></div>
                    <div class="spec-module-card safety"><div class="card-label"><span class="cn">安全内距</span><span class="en">Safety</span></div><div class="card-input-row"><el-input-number v-model="formData.dimensions.bleedInner" :step="0.5" :min="0" :controls="false" class="module-input" /><span class="unit">cm</span></div></div>
                  </div>
                </div>
              </div>
            </div>

            <div v-else-if="activeStep === 1" key="step2-product" class="step-panel product-def-panel">
              <div class="panel-header"><h2>产品定义</h2><p>请按顺序完善产品的核心身份信息与规格参数。</p></div>
              <div class="vertical-stack-container">
                <el-form-item prop="marketing.brand" class="stack-item">
                  <div class="standard-input-card">
                    <div class="icon-wrapper"><el-icon><Trophy /></el-icon></div>
                    <div class="content-wrapper">
                      <label>品牌名称 Brand</label>
                      <el-select v-model="formData.marketing.brand" placeholder="选择或输入品牌" class="seamless-input" filterable allow-create default-first-option :fit-input-width="true" @change="handleBrandChange">
                        <el-option v-for="item in brandOptions" :key="item.id" :label="`${item.name} - ${item.brand_category_name || '通用'}`" :value="item.name" />
                      </el-select>
                    </div>
                  </div>
                </el-form-item>
                <div class="grid-two-col">
                  <el-form-item class="stack-item"><div class="standard-input-card"><div class="icon-wrapper"><el-icon><OfficeBuilding /></el-icon></div><div class="content-wrapper"><label>制造商 Manufacturer</label><el-input v-model="formData.marketing.manufacturer" placeholder="自动关联或手动输入" class="seamless-input" /></div></div></el-form-item>
                  <el-form-item class="stack-item"><div class="standard-input-card"><div class="icon-wrapper"><el-icon><Location /></el-icon></div><div class="content-wrapper"><label>产地地址 Address</label><el-input v-model="formData.marketing.address" placeholder="自动关联或手动输入" class="seamless-input" /></div></div></el-form-item>
                </div>
                <div class="grid-two-col">
                  <el-form-item prop="marketing.capacityValue" class="stack-item"><div class="standard-input-card"><div class="icon-wrapper"><el-icon><Monitor /></el-icon></div><div class="content-wrapper"><label>正面含量 Net Wt (Front)</label><el-input v-model="formData.marketing.capacityValue" placeholder="e.g. 100g" class="seamless-input" /></div></div></el-form-item>
                  <el-form-item prop="marketing.capacityValueBack" class="stack-item"><div class="standard-input-card"><div class="icon-wrapper"><el-icon><Document /></el-icon></div><div class="content-wrapper"><label>背面含量 Net Wt (Back)</label><el-input v-model="formData.marketing.capacityValueBack" placeholder="同上或不同" class="seamless-input" /></div></div></el-form-item>
                </div>
                <el-form-item prop="marketing.sku" class="stack-item">
                  <div class="standard-input-card has-drawer">
                    <div class="main-row">
                      <div class="icon-wrapper"><el-icon><Ticket /></el-icon></div>
                      <div class="content-wrapper"><label>商品编码 SKU</label><el-input v-model="formData.marketing.sku" placeholder="输入编码后回车" class="seamless-input" @change="handleFetchBarcode" /></div>
                      <div class="status-indicator"><el-tag v-if="barcodeUrl" type="success" effect="dark" round size="small">已关联条码</el-tag><el-tag v-else-if="isFetchingBarcode" type="warning" effect="plain" round size="small">查找中...</el-tag><el-tag v-else type="info" effect="plain" round size="small">未关联</el-tag></div>
                    </div>
                    <div v-if="barcodeUrl" class="bottom-drawer"><div class="file-info"><el-icon><Picture /></el-icon><span>{{ barcodeUrl.split('/').pop() }}</span></div><el-link type="primary" :underline="false" :href="barcodeUrl" target="_blank">预览 <el-icon><Link /></el-icon></el-link></div>
                  </div>
                </el-form-item>
                <el-form-item prop="marketing.sellingPoints" class="stack-item" style="margin-top: 10px;">
                  <div class="selling-points-board">
                    <div class="board-header"><el-icon><StarFilled /></el-icon> <span>正面卖点文案 Selling Points</span></div>
                    <div class="tags-area">
                      <el-tag v-for="tag in formData.marketing.sellingPoints" :key="tag" closable effect="light" class="point-tag" @close="handleCloseTag(tag)">{{ tag }}</el-tag>
                      <el-input v-if="formData.marketing.sellingPoints.length < 6" v-model="inputValue" class="ghost-input-tag" placeholder="+ 输入卖点回车" @keyup.enter="handleInputConfirm" @blur="handleInputConfirm" />
                    </div>
                    <div class="quick-pick-bar"><span class="label">推荐:</span><span class="chip" @click="addQuickTag('Eco-Friendly')">🌿 Eco-Friendly</span><span class="chip" @click="addQuickTag('Cruelty Free')">🐰 Cruelty Free</span></div>
                  </div>
                </el-form-item>
              </div>
            </div>

            <div v-else-if="activeStep === 2" key="step3-doc" class="step-panel">
              <div class="panel-header"><h2>文案智能解析</h2><p>上传 Word 文档，智能提取文档关键信息。</p></div>
              <div class="panel-card">
                <div v-if="!isDocParsed" class="upload-zone">
                  <el-upload class="hero-upload" drag action="#" :auto-upload="false" :on-change="handleFileUpload" :show-file-list="false">
                    <div class="upload-content"><div class="icon-circle"><el-icon><DocumentAdd /></el-icon></div><h3>点击或拖拽上传文档</h3><p>支持 .docx 格式</p></div>
                  </el-upload>
                </div>
                <div v-else class="parsed-view">
                  <div class="doc-status-card">
                    <div class="icon-box"><el-icon><Document /></el-icon></div>
                    <div class="info-box"><div class="filename">{{ fileName }}</div><div class="status-row"><el-icon><CircleCheckFilled /></el-icon><span>AI 解析完成</span></div></div>
                    <el-button class="change-btn" type="primary" text bg size="small" @click="isDocParsed = false">重新上传</el-button>
                  </div>
                  <div class="data-grid">
                    <div class="data-group full"><label>产品标准名称</label><div class="data-value">{{ formData.content.productName || '-' }}</div></div>
                    <div class="data-group"><label>原产国</label><div class="data-value">{{ formData.content.origin || '-' }}</div></div>
                    <div class="data-group"><label>保质期</label><div class="data-value">{{ formData.content.shelfLife || '-' }}</div></div>
                    <div class="data-group full"><label>成分表</label><div class="data-value text-block">{{ formData.content.ingredients || '-' }}</div></div>
                  </div>
                </div>
              </div>
            </div>

            <div v-else-if="activeStep === 3" key="step4-ai" class="step-panel ai-design-panel">
              <div class="panel-header">
                <h2>AI 智能设计</h2>
                <p>调用 AI 生成效果图供设计师参考，并生成设计素材置入 PSD</p>
              </div>

              <div class="ai-studio-layout">
                <div class="benchmark-section">
                  <div class="section-label">对标产品图片 <span class="en">BENCHMARK IMAGE</span></div>
                  <div class="image-preview-card" :class="{ 'has-image': formData.aiDesign.benchmarkImage }">
                    <el-image
                        v-if="formData.aiDesign.benchmarkImage"
                        :src="formData.aiDesign.benchmarkImage"
                        fit="contain"
                        class="benchmark-img"
                        :preview-src-list="[formData.aiDesign.benchmarkImage]"
                    />
                    <div v-else class="empty-state">
                      <div class="dashed-circle"><el-icon><Picture /></el-icon></div>
                      <p>请输入 SKU 获取图片</p>
                    </div>
                    <div v-if="formData.aiDesign.benchmarkImage" class="img-overlay">
                      <el-tag effect="dark" type="info" size="small">Ref</el-tag>
                    </div>
                  </div>
                </div>

                <div class="design-control-section">
                  <div class="section-label">设计类型 <span class="en">DESIGN TYPE</span></div>
                  <div class="type-grid">
                    <div class="type-card" :class="{ active: formData.aiDesign.designType === 'logo' }" @click="formData.aiDesign.designType = 'logo'">
                      <div class="check-mark" v-if="formData.aiDesign.designType === 'logo'"><el-icon><Select /></el-icon></div>
                      <div class="icon-bubble"><el-icon><Refresh /></el-icon></div>
                      <div class="card-info">
                        <h3>替换LOGO</h3>
                        <p>仅替换LOGO，其他不变</p>
                      </div>
                    </div>
                    <div class="type-card" :class="{ active: formData.aiDesign.designType === 'similar' }" @click="formData.aiDesign.designType = 'similar'">
                      <div class="check-mark" v-if="formData.aiDesign.designType === 'similar'"><el-icon><Select /></el-icon></div>
                      <div class="icon-bubble"><el-icon><CopyDocument /></el-icon></div>
                      <div class="card-info">
                        <h3>相似的设计</h3>
                        <p>设计相似度 70%-80%</p>
                      </div>
                    </div>
                    <div class="type-card" :class="{ active: formData.aiDesign.designType === 'reference' }" @click="formData.aiDesign.designType = 'reference'">
                      <div class="check-mark" v-if="formData.aiDesign.designType === 'reference'"><el-icon><Select /></el-icon></div>
                      <div class="icon-bubble"><el-icon><Crop /></el-icon></div>
                      <div class="card-info">
                        <h3>参考设计</h3>
                        <p>设计相似度 30%-50%</p>
                      </div>
                    </div>
                    <div class="type-card" :class="{ active: formData.aiDesign.designType === 'auto' }" @click="formData.aiDesign.designType = 'auto'">
                      <div class="check-mark" v-if="formData.aiDesign.designType === 'auto'"><el-icon><Select /></el-icon></div>
                      <div class="icon-bubble"><el-icon><MagicStick /></el-icon></div>
                      <div class="card-info">
                        <h3>让 AI 设计</h3>
                        <p>AI 根据信息自由发挥</p>
                      </div>
                    </div>
                  </div>

                  <div class="ai-action-area">
                    <div class="divider"></div>
                    <el-button class="magic-btn" :loading="isPreviewLoading" @click="handleGeneratePreview">
                      <el-icon class="magic-icon"><MagicStick /></el-icon>
                      <span>{{ isPreviewLoading ? 'AI 正在绘制中...' : '生成效果图 Generate Preview' }}</span>
                    </el-button>
                  </div>
                </div>
              </div>
            </div>

            <div v-else-if="activeStep === 4" key="step5-review" class="step-panel centered-panel">
              <div class="success-visual"><div class="pulse-ring"></div><el-icon class="success-icon"><CircleCheckFilled /></el-icon></div>
              <h2>准备生成工程文件</h2>
              <p class="subtitle">所有数据校验通过，即将构建 PSD 刀版与图层结构。</p>
              <div class="summary-box">
                <div class="summary-item"><span>SKU</span><strong>{{ formData.marketing.sku }}</strong></div>
                <div class="summary-item"><span>品牌</span><strong>{{ formData.marketing.brand }}</strong></div>
                <div class="summary-item"><span>尺寸</span><strong>{{ formData.dimensions.length }} x {{ formData.dimensions.width }} x {{ formData.dimensions.height }}</strong></div>
              </div>
            </div>
          </transition>
        </el-form>

        <transition name="slide-fade" mode="out-in">
          <div v-if="activeStep === 5" key="step6-success" class="success-page">
            <div class="success-banner"><el-icon><Select /></el-icon></div>
            <h2>生成任务已完成！</h2>
            <p class="sub-msg">PSD 工程文件已自动下载到您的本地。</p>
            <div class="file-card">
              <el-icon class="file-icon"><Files /></el-icon>
              <div class="file-info"><span class="fname">{{ generatedFileName || formData.marketing.brand + '_' + formData.marketing.sku + '.psd' }}</span></div>
              <el-button class="re-download-btn" type="primary" plain round @click="triggerDownload(currentDownloadUrl)"><el-icon style="margin-right: 4px"><Download /></el-icon> 重新下载</el-button>
            </div>
            <div class="action-area">
              <el-button class="btn-lg" @click="resetWorkflow">新建项目</el-button>
              <el-button class="btn-lg" type="primary" plain @click="activeStep = 0">查看详情</el-button>
            </div>
          </div>
        </transition>
      </div>

      <footer class="workflow-footer" v-if="activeStep < 5">
        <div class="footer-glass-inner">
          <div class="nav-control">
            <transition name="el-fade-in">
              <el-button
                  v-if="activeStep > 0"
                  @click="prevStep"
                  class="nav-btn prev-btn"
              >
                <el-icon class="btn-icon-left"><ArrowLeft /></el-icon> 上一步
              </el-button>
            </transition>
          </div>

          <div class="action-control">
            <el-button
                v-if="activeStep < 4"
                type="primary"
                @click="nextStep"
                class="nav-btn next-btn"
            >
              下一步 <el-icon class="btn-icon-right"><ArrowRight /></el-icon>
            </el-button>

            <el-button
                v-if="activeStep === 4"
                type="primary"
                @click="handleGeneratePSD"
                class="nav-btn finish-btn"
                :loading="isGenerating"
            >
              <el-icon v-if="!isGenerating" class="btn-icon"><MagicStick /></el-icon>
              {{ isGenerating ? '正在构建工程...' : '生成设计文件' }}
            </el-button>
          </div>
        </div>
      </footer>

    </div>

    <el-dialog v-model="isGenerating" :show-close="false" width="380px" align-center class="design-gen-dialog">
      <template #header><div class="dialog-header-custom"><div class="icon-pulse"><el-icon><MagicStick /></el-icon></div><span class="header-title">正在生成设计文件</span></div></template>
      <div class="progress-dialog-content">
        <div class="progress-ring-wrapper"><el-progress type="circle" :percentage="progressPercentage" :status="progressStatus as any" :width="150" :stroke-width="10" color="#2563eb" :show-text="false" stroke-linecap="round" /><div class="ring-inner-text"><span class="number">{{ progressPercentage }}</span><span class="symbol">%</span></div></div>
        <div class="status-box"><p class="status-main">{{ progressStatus === 'success' ? '生成成功' : '处理中...' }}</p><p class="status-sub">{{ progressMessage }}</p></div>
      </div>
    </el-dialog>
  </div>
</template>

<script lang="ts" setup>
import { computed } from 'vue'
import {
  DocumentAdd, Trophy, Ticket, CircleCheckFilled, Select, Files,
  MagicStick, Download, OfficeBuilding, Location, Link, Monitor, Document, StarFilled,
  Picture, DArrowRight, DArrowLeft, Top, Scissor,
  FullScreen, Refresh, CopyDocument, Crop, ArrowLeft, ArrowRight
} from '@element-plus/icons-vue'
import { usePackagingConfig } from '../logic/usePackagingConfig'

defineProps<{ username: string }>()
const emit = defineEmits(['logout'])

const {
  activeStep, formRef, formData, rules, isDocParsed, fileName, inputValue, brandOptions,
  isGenerating, isPreviewLoading, progressPercentage, progressStatus, progressMessage, currentDownloadUrl, generatedFileName,
  isFetchingBarcode, barcodeUrl,
  nextStep, prevStep, handleFileUpload, handleCloseTag, handleInputConfirm, addQuickTag, handleGeneratePSD, triggerDownload, resetWorkflow, handleBrandChange, handleFetchBarcode, handleGeneratePreview
} = usePackagingConfig(() => emit('logout'))

const cubeStyle = computed(() => {
  const { length, width, height } = formData.dimensions
  const l = Math.max(length, 0.1); const w = Math.max(width, 0.1); const h = Math.max(height, 0.1)
  const maxSide = Math.max(l, w, h); const baseSize = 140; const scale = baseSize / maxSide
  return { '--box-l': `${l * scale}px`, '--box-w': `${w * scale}px`, '--box-h': `${h * scale}px` }
})
</script>

<style scoped lang="scss" src="../styles/PackagingConfig.scss"></style>