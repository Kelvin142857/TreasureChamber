<template>
  <div>
    <Toolbar class="mb-3">
      <template #start>
        <h4 class="font-bold m-0">意向单</h4>
      </template>
      <template #end>
        <Button label="导出 Excel" icon="pi pi-download" severity="secondary" outlined class="mr-2" @click="exportXlsx" />
        <Button label="新建意向单" icon="pi pi-plus" @click="router.push('/intent-orders/new')" />
      </template>
    </Toolbar>

    <div class="d-flex flex-wrap gap-2 mb-3 align-items-center">
      <SelectButton v-model="filters.status" :options="statusOptions" option-label="label" option-value="value" />
      <InputText v-model="filters.keyword" placeholder="搜索单号 / 客户姓名 / 电话" class="w-4" @keyup.enter="load(1)" />
      <Button label="搜索" icon="pi pi-search" outlined @click="load(1)" />
    </div>

    <DataTable :value="paged.items" :loading="loading" striped-rows data-key="id" class="p-datatable-sm">
      <Column field="orderNo" header="意向单号">
        <template #body="{ data }"><span class="font-bold">{{ data.orderNo }}</span></template>
      </Column>
      <Column field="customerName" header="客户" />
      <Column field="customerPhone" header="电话" />
      <Column header="产品数" style="width:90px">
        <template #body="{ data }"><Badge :value="data.items.length" severity="info" /></template>
      </Column>
      <Column header="状态" style="width:100px">
        <template #body="{ data }">
          <Tag :value="data.statusLabel" :severity="statusSeverity(data.status)" />
        </template>
      </Column>
      <Column header="创建时间" style="width:160px">
        <template #body="{ data }">
          <span class="text-muted small">{{ formatTime(data.createdAt) }}</span>
        </template>
      </Column>
      <Column header="操作" style="width:100px">
        <template #body="{ data }">
          <Button label="查看" icon="pi pi-eye" size="small" outlined @click="router.push(`/intent-orders/${data.id}`)" />
        </template>
      </Column>
    </DataTable>

    <Paginator v-if="paged.totalPages > 1" :rows="pageSize" :totalRecords="paged.total"
               :first="(paged.page - 1) * pageSize" @page="onPage" class="mt-2" />
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useToast } from 'primevue/usetoast'
import api, { downloadBlob } from '../api'

const router = useRouter()
const toast = useToast()
const pageSize = 20

const statusOptions = [
  { label: '全部', value: null },
  { label: '新建', value: 0 },
  { label: '跟进中', value: 1 },
  { label: '已成交', value: 2 },
  { label: '已放弃', value: 3 }
]
const filters = reactive({ status: null, keyword: '' })
const paged = ref({ items: [], total: 0, page: 1, totalPages: 0 })
const loading = ref(false)

onMounted(() => load(1))

async function load(page) {
  loading.value = true
  try {
    const params = { page, pageSize }
    if (filters.status !== null) params.status = filters.status
    if (filters.keyword) params.keyword = filters.keyword
    const { data } = await api.get('/intent-orders', { params })
    paged.value = data
  } finally {
    loading.value = false
  }
}
function onPage(e) { load(e.page + 1) }

function statusSeverity(s) {
  return { 0: 'info', 1: 'warning', 2: 'success', 3: 'secondary' }[s] || 'secondary'
}
function formatTime(iso) {
  const d = new Date(iso)
  const p = n => String(n).padStart(2, '0')
  return `${d.getFullYear()}-${p(d.getMonth() + 1)}-${p(d.getDate())} ${p(d.getHours())}:${p(d.getMinutes())}`
}

async function exportXlsx() {
  const res = await api.get('/intent-orders/export', { responseType: 'blob' })
  downloadBlob(res.data, `意向单_${new Date().toISOString().slice(0, 14).replace(/[-T:]/g, '')}.xlsx`,
    'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet')
}
</script>
