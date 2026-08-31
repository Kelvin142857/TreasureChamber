<template>
  <div>
    <Toolbar class="mb-3">
      <template #start>
        <h4 class="font-bold m-0">产品管理</h4>
      </template>
      <template #end>
        <Button label="新建产品" icon="pi pi-plus" class="mr-2" @click="openCreate" />
        <Button label="批量导入" icon="pi pi-upload" severity="secondary" outlined @click="router.push('/import')" />
      </template>
    </Toolbar>

    <div class="d-flex gap-2 mb-3" style="max-width: 640px;">
      <Dropdown v-model="filters.seriesId" :options="series" option-label="name" option-value="id"
                placeholder="全部系列" class="w-4" show-clear @change="load(1)" />
      <InputText v-model="filters.keyword" placeholder="搜索型号 / 名称" class="flex-1" @keyup.enter="load(1)" />
      <Button label="筛选" icon="pi pi-filter" outlined @click="load(1)" />
    </div>

    <DataTable :value="paged.items" :loading="loading" striped-rows data-key="id" class="p-datatable-sm">
      <Column header="图片" style="width:70px">
        <template #body="{ data }">
          <img v-if="data.imagePath" :src="'/' + data.imagePath" class="rounded" style="width:46px;height:46px;object-fit:cover" alt="" />
        </template>
      </Column>
      <Column field="model" header="型号" :sortable="true">
        <template #body="{ data }"><span class="font-bold">{{ data.model }}</span></template>
      </Column>
      <Column field="name" header="名称" />
      <Column field="seriesName" header="系列" />
      <Column field="categoryName" header="分类" />
      <Column header="状态" style="width:90px">
        <template #body="{ data }">
          <Tag v-if="data.isActive" value="在售" severity="success" />
          <Tag v-else value="停用" severity="secondary" />
        </template>
      </Column>
      <Column header="操作" style="width:210px">
        <template #body="{ data }">
          <Button icon="pi pi-eye" text rounded title="查看" @click="router.push(`/product/${data.id}`)" />
          <Button icon="pi pi-pencil" text rounded title="编辑" @click="openEdit(data.id)" />
          <Button icon="pi pi-trash" text rounded severity="danger" title="删除" @click="remove(data)" />
        </template>
      </Column>
    </DataTable>

    <Paginator v-if="paged.totalPages > 1" :rows="pageSize" :totalRecords="paged.total"
               :first="(paged.page - 1) * pageSize" @page="onPage" class="mt-2" />

    <ProductEditDialog v-model:visible="editVisible" :product-id="editId" @saved="load(paged.page)" />
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useConfirm } from 'primevue/useconfirm'
import { useToast } from 'primevue/usetoast'
import api from '../api'
import ProductEditDialog from './ProductEditDialog.vue'

const router = useRouter()
const confirm = useConfirm()
const toast = useToast()
const pageSize = 20

const filters = reactive({ seriesId: null, keyword: '' })
const series = ref([])
const paged = ref({ items: [], total: 0, page: 1, totalPages: 0 })
const loading = ref(false)
const editVisible = ref(false)
const editId = ref(null)

onMounted(async () => {
  const { data } = await api.get('/catalog')
  series.value = data.series
  load(1)
})

async function load(page) {
  loading.value = true
  try {
    const { data } = await api.get('/products', {
      params: { ...filters, page, pageSize, includeInactive: true }
    })
    paged.value = data
  } finally {
    loading.value = false
  }
}
function onPage(e) { load(e.page + 1) }
function openCreate() {
  editId.value = null
  editVisible.value = true
}
function openEdit(id) {
  editId.value = id
  editVisible.value = true
}
function remove(product) {
  confirm.require({
    message: `确认删除产品 ${product.model}？删除后不可恢复。`,
    header: '删除确认',
    icon: 'pi pi-exclamation-triangle',
    acceptLabel: '删除',
    rejectLabel: '取消',
    accept: async () => {
      await api.delete(`/products/${product.id}`)
      toast.add({ severity: 'success', summary: '已删除', life: 2000 })
      load(paged.value.page)
    }
  })
}
</script>
