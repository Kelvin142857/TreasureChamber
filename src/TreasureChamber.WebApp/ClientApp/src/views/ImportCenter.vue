<template>
  <div>
    <h4 class="font-bold mb-3">批量导入</h4>

    <div class="grid">
      <!-- 目录导入 -->
      <div class="col-12 lg:col-7">
        <Card>
          <template #title>1. 上传产品目录（Excel / CSV）</template>
          <template #content>
            <p class="text-muted small mb-3">
              支持 <code>.xlsx</code> 与 <code>.csv</code>（兼容中文 Excel 导出的 ANSI/GBK 编码）。
              表头：<b>型号*</b>、名称、系列、分类（支持「灯具/吊灯」多级）、描述、规格参数（格式：<code>功率=36W;色温=3000K</code>）。
              型号已存在的产品自动更新，新型号自动新增。
            </p>
            <FileUpload name="file" accept=".xlsx,.csv" choose-label="选择文件" mode="basic" custom-upload
                        :auto="false" @select="onSelect" class="mb-3" />
            <div class="mb-3">
              <a class="no-print" href="/api/import/template" download="产品导入模板.xlsx">
                <Button label="下载导入模板" icon="pi pi-download" severity="secondary" outlined size="small" />
              </a>
              <Button label="批量图片导入" icon="pi pi-images" severity="primary" outlined size="small" class="ml-2" @click="goImages" />
            </div>

            <Message v-if="previewError" severity="error" :closable="false" class="mb-3">{{ previewError }}</Message>

            <!-- 预览结果 -->
            <template v-if="preview">
              <div class="d-flex gap-2 mb-3">
                <Tag :value="`新增 ${preview.newCount}`" severity="info" />
                <Tag :value="`更新 ${preview.updateCount}`" severity="warning" />
                <Tag :value="`错误 ${preview.errorCount}`" severity="danger" />
                <span class="text-muted small align-self-center">文件：{{ preview.fileName }}</span>
              </div>
              <Message v-if="preview.truncated" severity="warn" class="mb-2 py-1">文件较大，仅显示前 300 行，确认导入时仍会处理全部行。</Message>
              <DataTable :value="preview.rows" :rows="10" paginator class="p-datatable-sm" style="max-height:420px;overflow:auto">
                <Column field="rowNumber" header="行" style="width:60px" />
                <Column field="model" header="型号" />
                <Column field="name" header="名称" />
                <Column field="series" header="系列" />
                <Column field="category" header="分类" />
                <Column header="操作" style="width:90px">
                  <template #body="{ data }">
                    <Tag :value="actionLabel(data.action)" :severity="actionSeverity(data.action)" />
                  </template>
                </Column>
                <Column field="error" header="说明">
                  <template #body="{ data }"><span class="text-danger small">{{ data.error || '' }}</span></template>
                </Column>
              </DataTable>
              <div class="mt-3">
                <Button label="确认导入" icon="pi pi-check" severity="success" :loading="committing" @click="commit" />
                <Button label="重新选择" severity="secondary" outlined class="ml-2" @click="resetPreview" />
              </div>
            </template>

            <!-- 导入结果 -->
            <div v-if="result" class="text-center py-3">
              <i class="pi pi-check-circle" style="font-size:3rem;color:#22c55e"></i>
              <h5 class="font-bold mt-2">导入完成</h5>
              <div class="d-flex justify-content-center gap-2 my-3">
                <Tag :value="`新增 ${result.added}`" severity="info" />
                <Tag :value="`更新 ${result.updated}`" severity="warning" />
                <Tag :value="`跳过 ${result.skipped}`" severity="secondary" />
              </div>
              <div class="mt-3">
                <Button label="查看产品列表" icon="pi pi-database" @click="router.push('/manage')" />
              </div>
            </div>
          </template>
        </Card>
      </div>

      <!-- 批量图片 -->
      <div class="col-12 lg:col-5">
        <Card>
          <template #title>2. 批量图片导入</template>
          <template #content>
            <p class="text-muted small mb-3">
              图片文件名需以<b>产品型号</b>开头（如 <code>XD-1001-1.jpg</code>、<code>XD-1001 主图.png</code>），
              系统自动匹配型号归档到对应产品。
            </p>
            <div id="imagesSection">
              <FileUpload name="files" accept="image/*" multiple choose-label="选择图片" custom-upload
                          @select="onImagesSelect" />
              <Button v-if="pendingImages.length" label="上传并匹配" icon="pi pi-upload" class="mt-2" :loading="uploading" @click="uploadImages" />
            </div>
            <template v-if="imagesResult">
              <Divider />
              <Message :severity="imagesResult.unmatched.length ? 'warn' : 'success'" :closable="false" class="py-2">
                成功上传并匹配 {{ imagesResult.uploaded }} 张图片
              </Message>
              <h6 v-if="imagesResult.matched.length" class="font-bold mt-3">匹配成功</h6>
              <ul class="small mt-1">
                <li v-for="(m, i) in imagesResult.matched" :key="i">{{ m.fileName }} → 型号 {{ m.model }}</li>
              </ul>
              <h6 v-if="imagesResult.unmatched.length" class="font-bold text-danger mt-3">未匹配（型号不存在或文件名格式不对）</h6>
              <ul class="small text-danger mt-1">
                <li v-for="(u, i) in imagesResult.unmatched" :key="i">{{ u }}</li>
              </ul>
            </template>
          </template>
        </Card>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useToast } from 'primevue/usetoast'
import api from '../api'

const router = useRouter()
const toast = useToast()

const previewError = ref('')
const preview = ref(null)
const selectedFile = ref(null)
const committing = ref(false)
const result = ref(null)

const pendingImages = ref([])
const uploading = ref(false)
const imagesResult = ref(null)

function onSelect(e) {
  selectedFile.value = e.files[0]
  preview.value = null
  result.value = null
  previewError.value = ''
  uploadPreview()
}

async function uploadPreview() {
  if (!selectedFile.value) return
  previewError.value = ''
  const fd = new FormData()
  fd.append('file', selectedFile.value)
  try {
    const { data } = await api.post('/import/preview', fd)
    preview.value = data
  } catch (e) {
    previewError.value = e.response?.data || e.message
  }
}

async function commit() {
  committing.value = true
  try {
    const { data } = await api.post('/import/commit')
    result.value = data
    preview.value = null
    selectedFile.value = null
  } finally {
    committing.value = false
  }
}

function resetPreview() {
  preview.value = null
  selectedFile.value = null
  result.value = null
}

function actionLabel(a) {
  return { New: '新增', Update: '更新', Error: '错误' }[a] || a
}
function actionSeverity(a) {
  return { New: 'info', Update: 'warning', Error: 'danger' }[a] || 'secondary'
}

function onImagesSelect(e) {
  pendingImages.value = e.files
}

async function uploadImages() {
  uploading.value = true
  try {
    const fd = new FormData()
    pendingImages.value.forEach(f => fd.append('files', f))
    const { data } = await api.post('/import/images', fd)
    imagesResult.value = data
    toast.add({ severity: 'success', summary: '上传完成', life: 2000 })
  } finally {
    uploading.value = false
  }
}

function goImages() {
  document.getElementById('imagesSection').scrollIntoView({ behavior: 'smooth' })
}
</script>
