<template>
  <div>
    <h4 class="font-bold mb-1">二维码中心</h4>
    <p class="text-muted small mb-3">
      勾选产品后「打印标签」生成 A4 打印页，或「下载 PNG」打包为 zip。
      二维码内容为「系统设置中的访问地址 + 产品详情页」，请先在
      <a class="text-primary" @click="router.push('/settings')">系统设置</a>确认局域网地址。
    </p>

    <div class="d-flex gap-2 mb-3" style="max-width: 720px;">
      <Dropdown v-model="filters.seriesId" :options="series" option-label="name" option-value="id"
                placeholder="全部系列" class="w-4" show-clear @change="load(1)" />
      <InputText v-model="filters.keyword" placeholder="搜索型号 / 名称" class="flex-1" @keyup.enter="load(1)" />
      <Button label="筛选" icon="pi pi-filter" outlined @click="load(1)" />
    </div>

    <div class="d-flex gap-2 mb-2">
      <Button label="打印所选标签" icon="pi pi-print" @click="printSelected" :disabled="!selected.length" />
      <Button label="下载所选 PNG" icon="pi pi-download" severity="secondary" outlined @click="zipSelected" :disabled="!selected.length" />
      <div class="align-self-center small">
        <Checkbox v-model="selectAll" binary input-id="selectAll" class="mr-1" />
        <label for="selectAll">全选本页</label>
      </div>
    </div>

    <DataTable v-model:selection="selected" :value="paged.items" data-key="id" :loading="loading"
               striped-rows selection-mode="multiple" class="p-datatable-sm" @row-select="onRowSelect" @row-unselect="onRowUnselect">
      <Column selection-mode="multiple" header-style="width:3rem" />
      <Column field="model" header="型号" :sortable="true">
        <template #body="{ data }"><span class="font-bold">{{ data.model }}</span></template>
      </Column>
      <Column field="name" header="名称" />
      <Column field="seriesName" header="系列" />
      <Column field="categoryName" header="分类" />
    </DataTable>

    <Paginator v-if="paged.totalPages > 1" :rows="pageSize" :totalRecords="paged.total"
               :first="(paged.page - 1) * pageSize" @page="onPage" class="mt-2" />
  </div>
</template>

<script setup>
import { ref, reactive, onMounted, watch } from 'vue'
import { useRouter } from 'vue-router'
import api, { downloadBlob } from '../api'

const router = useRouter()
const pageSize = 20
const filters = reactive({ seriesId: null, keyword: '' })
const series = ref([])
const paged = ref({ items: [], total: 0, page: 1, totalPages: 0 })
const loading = ref(false)
const selected = ref([])
const selectAll = ref(false)

onMounted(async () => {
  const { data } = await api.get('/catalog')
  series.value = data.series
  load(1)
})

async function load(page) {
  loading.value = true
  try {
    const { data } = await api.get('/products', { params: { ...filters, page, pageSize } })
    paged.value = data
    selectAll.value = false
  } finally {
    loading.value = false
  }
}
function onPage(e) { load(e.page + 1) }

watch(selectAll, (v) => {
  selected.value = v ? [...paged.value.items] : []
})
function onRowSelect() { syncSelectAll() }
function onRowUnselect() { syncSelectAll() }
function syncSelectAll() {
  selectAll.value = selected.value.length === paged.value.items.length && paged.value.items.length > 0
}

function printSelected() {
  window.open(`/qr/print?ids=${selected.value.map(s => s.id).join(',')}`, '_blank')
}

async function zipSelected() {
  const res = await api.get(`/qr/zip?ids=${selected.value.map(s => s.id).join(',')}`, { responseType: 'blob' })
  downloadBlob(res.data, `产品二维码_${new Date().toISOString().slice(0, 14).replace(/[-T:]/g, '')}.zip`, 'application/zip')
}
</script>
